﻿using FirstWebAPI.Data;
using FirstWebAPI.Interfaces;
using FirstWebAPI.Models;

namespace FirstWebAPI.Repository
{
    public class CountryRepository : ICountryRepository
    {
        private readonly DataContext _context;

        public CountryRepository(DataContext context) 
        {
            _context = context;
        }

        public bool CountryExists(int id)
        {
            return _context.Countries.Any(c => c.Id == id);
        }

        public ICollection<Country> GetCountries()
        {
            return _context.Countries.ToList();
        }

        public Country GetCountry(int id)
        {
            return _context.Countries.Where(c => c.Id == id).FirstOrDefault();
        }

        public Country GetCountryByOwner(int ownerId)
        {
            return _context.Owners.Where(o => o.Id == ownerId).Select(o => o.Country).FirstOrDefault();
        }

        public ICollection<Owner> GetOwnersByCountry(int countryId)
        {
            //return _context.Countries.Where(c => c.Id == countryId).Select(c => c.Owners).FirstOrDefault();
            //OR
            return _context.Owners.Where(o => o.Country.Id == countryId).ToList();
        }
    }
}