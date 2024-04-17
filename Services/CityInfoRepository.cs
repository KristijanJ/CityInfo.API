using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services;
public class CityInfoRepository : ICityInfoRepository
{
  private readonly CityInfoContext _context;

  public CityInfoRepository(CityInfoContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }
  public async Task<IEnumerable<City>> GetCitiesAsync()
  {
    return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
  }

  public async Task<IEnumerable<City>> GetCitiesAsync(
    string? name, string? searchQuery, int pageNumber, int pageSize)
  {
    var collection = _context.Cities as IQueryable<City>;

    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      collection = collection.Where(city => city.Name == name);
    }

    if (!string.IsNullOrWhiteSpace(searchQuery))
    {
      searchQuery = searchQuery.Trim();
      collection = collection.Where(
        city => city.Name.Contains(searchQuery) ||
        (city.Description != null && city.Description.Contains(searchQuery))
      );
    }

    return await collection
      .OrderBy(city => city.Name)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .ToListAsync();
  }

  public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
  {
    if (includePointsOfInterest)
    {
      return await _context.Cities.Include(c => c.PointsOfInterest)
        .Where(c => c.Id == cityId).FirstOrDefaultAsync();
    }

    return await _context.Cities
      .Where(c => c.Id == cityId).FirstOrDefaultAsync();
  }

  async Task<bool> ICityInfoRepository.CityExistsAsync(int cityId)
  {
    return await _context.Cities.AnyAsync(c => c.Id == cityId);
  }

  public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
  {
    return await _context.PointsOfInterest
      .Where(p => p.CityId == cityId).ToListAsync();
  }

  public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointId)
  {
    return await _context.PointsOfInterest
      .Where(p => p.CityId == cityId && p.Id == pointId).FirstOrDefaultAsync();
  }

  public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
  {
    var city = await GetCityAsync(cityId, false);
    city?.PointsOfInterest.Add(pointOfInterest);
  }

  public async Task<bool> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync() >= 0;
  }

  public void DeletePointOfInterest(PointOfInterest pointOfInterest)
  {
    _context.PointsOfInterest.Remove(pointOfInterest);
  }
}
