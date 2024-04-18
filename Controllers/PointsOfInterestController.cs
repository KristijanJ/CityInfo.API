using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[ApiController]
// [Authorize(Policy = "MustBeFromAntwerp")]
[Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
[ApiVersion(2)]
public class PointsOfInterestController : ControllerBase
{
  private readonly ILogger<PointsOfInterestController> _logger;
  private readonly IMailService _mailService;
  private readonly ICityInfoRepository _cityInfoRepository;
  private readonly IMapper _mapper;

  public PointsOfInterestController(
    ILogger<PointsOfInterestController> logger,
    IMailService mailService,
    ICityInfoRepository cityInfoRepository,
    IMapper mapper
  )
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
    _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
    _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
  {
    // Get points of interest only for the user's City
    // string? cityName = User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;
    // if (!await _cityInfoRepository.CityNameMatchesCityId(cityName, cityId))
    // {
    //   return Forbid();
    // }

    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest");
      return NotFound();
    }

    var pointsEntity = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);

    return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsEntity));
  }

  [HttpGet("{pointId}", Name = "GetPointOfInterest")]
  public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointId)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      return NotFound();
    }

    var pointEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointId);

    if (pointEntity == null)
    {
      return NotFound();
    }

    return Ok(_mapper.Map<PointOfInterestDto>(pointEntity));
  }

  [HttpPost]
  public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(
    int cityId,
    PointOfInterestForCreationDto pointOfInterest
  )
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      return NotFound();
    }

    var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointOfInterest);

    await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
    await _cityInfoRepository.SaveChangesAsync();

    var createdPointOfInterest = _mapper.Map<PointOfInterestDto>(finalPointOfInterest);

    return CreatedAtRoute("GetPointOfInterest",
      new
      { cityId, pointId = createdPointOfInterest.Id },
      createdPointOfInterest
    );
  }

  [HttpPut("{pointId}")]
  public async Task<ActionResult> UpdatePointOfInterest(
    int cityId,
    int pointId,
    PointOfInterestForUpdateDto pointOfInterest)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      return NotFound();
    }

    var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointId);
    if (pointOfInterestEntity == null)
    {
      return NotFound();
    }

    _mapper.Map(pointOfInterest, pointOfInterestEntity);

    await _cityInfoRepository.SaveChangesAsync();

    return NoContent();
  }

  [HttpPatch("{pointId}")]
  public async Task<ActionResult> PartiallyUpdatePointOfInterest(
    int cityId, int pointId,
    JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      return NotFound();
    }

    var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointId);
    if (pointOfInterestEntity == null)
    {
      return NotFound();
    }

    var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

    patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    if (!TryValidateModel(pointOfInterestToPatch))
    {
      return BadRequest(ModelState);
    }

    _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

    await _cityInfoRepository.SaveChangesAsync();

    return NoContent();
  }

  [HttpDelete("{pointId}")]
  public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointId)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      return NotFound();
    }

    var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointId);
    if (pointOfInterestEntity == null)
    {
      return NotFound();
    }

    _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
    await _cityInfoRepository.SaveChangesAsync();

    _mailService.Send(
      "Point of interest deleted",
      $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted."
    );

    return NoContent();
  }
}
