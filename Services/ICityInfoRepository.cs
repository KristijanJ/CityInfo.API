using CityInfo.API.Entities;
using CityInfo.API.Models;

namespace CityInfo.API.Services;
public interface ICityInfoRepository
{
  Task<IEnumerable<City>> GetCitiesAsync();
  Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
  Task<bool> CityExistsAsync(int cityId);
  Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);
  Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointId);
  Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);
  void DeletePointOfInterest(PointOfInterest pointOfInterest);
  Task<bool> SaveChangesAsync();
}