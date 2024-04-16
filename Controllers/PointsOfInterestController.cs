using CityInfo.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[ApiController]
[Route("api/cities/{cityId}/pointsofinterest")]
public class PointsOfInterestController : ControllerBase
{
  private readonly ILogger<PointsOfInterestController> _logger;

  public PointsOfInterestController(ILogger<PointsOfInterestController> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  [HttpGet]
  public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
  {
    CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);

    if (city == null)
    {
      _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest");
      return NotFound();
    }

    return Ok(city.PointsOfInterest);
  }

  [HttpGet("{pointId}", Name = "GetPointOfInterest")]
  public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointId)
  {
    CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);

    if (city == null)
    {
      return NotFound();
    }

    PointOfInterestDto? pointOfInterest = city.PointsOfInterest.FirstOrDefault(point => point.Id == pointId);

    if (pointOfInterest == null)
    {
      return NotFound();
    }

    return Ok(pointOfInterest);
  }

  [HttpPost]
  public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
  {
    CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);

    if (city == null)
    {
      return NotFound();
    }

    int maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(city => city.PointsOfInterest).Max(point => point.Id);

    PointOfInterestDto newPointOfInterest = new()
    {
      Id = ++maxPointOfInterestId,
      Name = pointOfInterest.Name,
      Description = pointOfInterest.Description,
    };

    city.PointsOfInterest.Add(newPointOfInterest);

    return CreatedAtRoute("GetPointOfInterest",
      new
      {
        cityId = cityId,
        pointId = newPointOfInterest.Id,
      },
      newPointOfInterest
    );
  }

  [HttpPut("{pointId}")]
  public ActionResult UpdatePointOfInterest(int cityId, int pointId, PointOfInterestForUpdateDto pointOfInterest)
  {
    CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);

    if (city == null)
    {
      return NotFound();
    }

    PointOfInterestDto? newPointOfInterest = city.PointsOfInterest.FirstOrDefault(point => point.Id == pointId);

    if (newPointOfInterest == null)
    {
      return NotFound();
    }

    newPointOfInterest.Name = pointOfInterest.Name;
    newPointOfInterest.Description = pointOfInterest.Description;

    return NoContent();
  }

  [HttpPatch("{pointId}")]
  public ActionResult PartiallyUpdatePointOfInterest(
    int cityId, int pointId,
    JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
  {
    CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);

    if (city == null)
    {
      return NotFound();
    }

    PointOfInterestDto? pointOfInterest = city.PointsOfInterest.FirstOrDefault(point => point.Id == pointId);

    if (pointOfInterest == null)
    {
      return NotFound();
    }

    PointOfInterestForUpdateDto pointOfInterestToPatch = new()
    {
      Name = pointOfInterest.Name,
      Description = pointOfInterest.Description,
    };

    patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    if (!TryValidateModel(pointOfInterestToPatch))
    {
      return BadRequest(ModelState);
    }

    pointOfInterest.Name = pointOfInterestToPatch.Name;
    pointOfInterest.Description = pointOfInterestToPatch.Description;

    return NoContent();
  }

  [HttpDelete("{pointId}")]
  public ActionResult DeletePointOfInterest(int cityId, int pointId)
  {
    CityDto? city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);

    if (city == null)
    {
      return NotFound();
    }

    PointOfInterestDto? pointOfInterest = city.PointsOfInterest.FirstOrDefault(point => point.Id == pointId);

    if (pointOfInterest == null)
    {
      return NotFound();
    }

    city.PointsOfInterest.Remove(pointOfInterest);

    return NoContent();
  }
}
