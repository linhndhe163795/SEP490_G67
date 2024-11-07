//using Microsoft.EntityFrameworkCore;
//using MyAPI.DTOs.VehicleOwnerDTOs;
//using MyAPI.Infrastructure.Interfaces;
//using MyAPI.Models;
//using System.Linq.Expressions;

//public class VehicleOwnerRepository : IVehicleOwnerRepository
//{
//    private readonly SEP490_G67Context _context;

//    public VehicleOwnerRepository(SEP490_G67Context context)
//    {
//        _context = context;
//    }

//    public async Task<VehicleOwner> Add(VehicleOwner entity)
//    {
//        await _context.VehicleOwners.AddAsync(entity);
//        await SaveChange();
//        return entity;
//    }

//    public async Task<VehicleOwner> Delete(VehicleOwner entity)
//    {
//        _context.VehicleOwners.Remove(entity);
//        await SaveChange();
//        return entity;
//    }

//    public async Task<VehicleOwner> Get(int id)
//    {
//        return await _context.VehicleOwners.FindAsync(id);
//    }

//    public async Task<VehicleOwner> Update(VehicleOwner entity)
//    {
//        _context.VehicleOwners.Update(entity);
//        await SaveChange();
//        return entity;
//    }

//    public async Task<IEnumerable<VehicleOwner>> GetAll()
//    {
//        return await _context.VehicleOwners.ToListAsync();
//    }

//    public async Task<IEnumerable<VehicleOwner>> Find(Expression<Func<VehicleOwner, bool>> predicate)
//    {
//        return await _context.VehicleOwners.Where(predicate).ToListAsync();
//    }

//    public async Task SaveChange()
//    {
//        await _context.SaveChangesAsync();
//    }


//    public async Task<VehicleOwner> CreateOwnerAsync(VehicleOwnerDTO vehicleOwnerDto)
//    {
//        var vehicleOwner = new VehicleOwner
//        {
//            Username = vehicleOwnerDto.Username,
//            Password = vehicleOwnerDto.Password,
//            Email = vehicleOwnerDto.Email,
//            NumberPhone = vehicleOwnerDto.NumberPhone,
//            FullName = vehicleOwnerDto.FullName,
//            Address = vehicleOwnerDto.Address,
//            Avatar = vehicleOwnerDto.Avatar,
//            Status = vehicleOwnerDto.Status,
//            Dob = vehicleOwnerDto.Dob,
//            CreatedAt = DateTime.Now,
//            UpdateAt = DateTime.Now
//        };

//        await _context.VehicleOwners.AddAsync(vehicleOwner);
//        await _context.SaveChangesAsync();

//        return vehicleOwner;
//    }

//    public async Task<VehicleOwner> UpdateOwnerAsync(int id, VehicleOwnerDTO vehicleOwnerDto)
//    {
//        var vehicleOwner = await _context.VehicleOwners.FindAsync(id);
//        if (vehicleOwner == null)
//        {
//            return null;
//        }

//        vehicleOwner.Username = vehicleOwnerDto.Username;
//        vehicleOwner.Password = vehicleOwnerDto.Password;
//        vehicleOwner.Email = vehicleOwnerDto.Email;
//        vehicleOwner.NumberPhone = vehicleOwnerDto.NumberPhone;
//        vehicleOwner.FullName = vehicleOwnerDto.FullName;
//        vehicleOwner.Address = vehicleOwnerDto.Address;
//        vehicleOwner.Avatar = vehicleOwnerDto.Avatar;
//        vehicleOwner.Status = vehicleOwnerDto.Status;
//        vehicleOwner.Dob = vehicleOwnerDto.Dob;
//        vehicleOwner.UpdateAt = DateTime.Now;

//        _context.VehicleOwners.Update(vehicleOwner);
//        await _context.SaveChangesAsync();

//        return vehicleOwner;
//    }




//}
