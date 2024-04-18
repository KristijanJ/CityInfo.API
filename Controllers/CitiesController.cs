using System.Text.Json;
using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[ApiController]
// [Authorize]
[Route("api/v{version:apiVersion}/cities")]
[ApiVersion(1)]
[ApiVersion(2)]
public class CitiesController : ControllerBase
{
  private readonly ICityInfoRepository _cityInfoRepository;
  private readonly IMapper _mapper;
  const int maxCitiesPageSize = 20;

  public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
  {
    _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
    _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(
    [FromQuery] string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
  {
    if (pageSize > maxCitiesPageSize)
    {
      pageSize = maxCitiesPageSize;
    }

    var (cityEntities, paginationMetadata) = await _cityInfoRepository
      .GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

    return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
  {
    var cityEntity = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);

    if (cityEntity == null)
    {
      return NotFound();
    }

    if (includePointsOfInterest)
    {
      return Ok(_mapper.Map<CityDto>(cityEntity));
    }

    return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(cityEntity));
  }
}