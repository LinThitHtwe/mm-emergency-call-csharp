﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MMEmergencyCall.Shared;

namespace MMEmergencyCall.Domain.Admin.Features.EmergencyServices;

public class AdminEmergencyServicesService
{
    private readonly ILogger<AdminEmergencyServicesService> _logger;

    private readonly AppDbContext _db;

    public AdminEmergencyServicesService(
        ILogger<AdminEmergencyServicesService> logger,
        AppDbContext db
    )
    {
        _logger = logger;
        _db = db;
    }

    public async Task<
        Result<List<AdminEmergencyServicesResponseModel>>
    > GetAllEmergencyServicesAsync()
    {
        try
        {
            var lst = await _db.EmergencyServices.ToListAsync();

            if (lst.Count == 0)
            {
                return Result<List<AdminEmergencyServicesResponseModel>>.NotFoundError();
            }

            var model = lst.Select(m => new AdminEmergencyServicesResponseModel
                {
                    ServiceId = m.ServiceId,
                    ServiceType = m.ServiceType,
                    ServiceGroup = m.ServiceGroup,
                    ServiceName = m.ServiceName,
                    PhoneNumber = m.PhoneNumber,
                    Location = m.Location,
                    Availability = m.Availability,
                    TownshipCode = m.TownshipCode,
                    ServiceStatus = m.ServiceStatus,
                })
                .ToList();

            return Result<List<AdminEmergencyServicesResponseModel>>.Success(model);
        }
        catch (Exception ex)
        {
            return Result<List<AdminEmergencyServicesResponseModel>>.Failure(ex.ToString());
        }
    }

    public async Task<
        Result<List<AdminEmergencyServicesResponseModel>>
    > GetEmergencyServicesByStatus(string status)
    {
        try
        {
            var serviceStatus = EnumServiceStatus.None;

            if (!string.IsNullOrEmpty(status))
            {
                serviceStatus = Enum.Parse<EnumServiceStatus>(status, true);
            }

            var query = _db.EmergencyServices.AsQueryable();

            if (serviceStatus != EnumServiceStatus.None)
            {
                query = query.Where(x => x.ServiceStatus == status);
            }

            var emergencyServices = await query.ToListAsync();

            if (emergencyServices == null)
            {
                return Result<List<AdminEmergencyServicesResponseModel>>.Failure(
                    "Emergency Service with service status: " + status + " not found."
                );
            }

            var model = emergencyServices
                .Select(emergencyService => new AdminEmergencyServicesResponseModel
                {
                    ServiceId = emergencyService.ServiceId,
                    UserId = emergencyService.UserId,
                    ServiceGroup = emergencyService.ServiceGroup,
                    ServiceType = emergencyService.ServiceType,
                    ServiceName = emergencyService.ServiceName,
                    PhoneNumber = emergencyService.PhoneNumber,
                    Location = emergencyService.Location,
                    Availability = emergencyService.Availability,
                    TownshipCode = emergencyService.TownshipCode,
                    ServiceStatus = emergencyService.ServiceStatus,
                })
                .ToList();

            return Result<List<AdminEmergencyServicesResponseModel>>.Success(model);
        }
        catch (System.ArgumentException)
        {
            string message = "Service Status must be Pending, Approved or Rejected";
            return Result<List<AdminEmergencyServicesResponseModel>>.Failure(message);
        }
        catch (Exception ex)
        {
            string message =
                "An error occurred while getting emergency service by service status: "
                + ex.ToString();
            _logger.LogError(message);

            return Result<List<AdminEmergencyServicesResponseModel>>.Failure(message);
        }
    }

    public async Task<
        Result<AdminEmergencyServicesPaginationResponseModel>
    > GetEmergencyServicesByStatusPaginationAsync(string status, int pageNo, int pageSize)
    {
        if (pageNo < 1 || pageSize < 1)
        {
            return Result<AdminEmergencyServicesPaginationResponseModel>.Failure(
                "Invalid page number or page size"
            );
        }

        var serviceStatus = EnumServiceStatus.None;

        if (!status.IsNullOrEmpty())
        {
            serviceStatus = Enum.Parse<EnumServiceStatus>(status, true);
        }

        try
        {
            var query = _db.EmergencyServices.AsQueryable();

            if (!serviceStatus.Equals(EnumServiceStatus.None))
            {
                query = query.Where(x => x.ServiceStatus == status);
            }

            var servicesByStatus = await query.ToListAsync();

            int rowCount = servicesByStatus.Count();

            if (rowCount is 0)
            {
                return Result<AdminEmergencyServicesPaginationResponseModel>.NotFoundError(
                    $"There is no data with the status: {status}"
                );
            }

            int pageCount = rowCount / pageSize;

            if (pageNo > pageCount)
            {
                return Result<AdminEmergencyServicesPaginationResponseModel>.Failure(
                    "Invalid page number"
                );
            }

            var lst = await query.Skip((pageNo - 1) * pageSize).Take(pageSize).ToListAsync();

            var data = lst.Select(x => new AdminEmergencyServicesResponseModel
                {
                    ServiceId = x.ServiceId,
                    UserId = x.UserId,
                    ServiceGroup = x.ServiceGroup,
                    ServiceType = x.ServiceType,
                    ServiceName = x.ServiceName,
                    PhoneNumber = x.PhoneNumber,
                    Location = x.Location,
                    Availability = x.Availability,
                    TownshipCode = x.TownshipCode,
                    ServiceStatus = x.ServiceStatus
                })
                .ToList();

            var model = new AdminEmergencyServicesPaginationResponseModel()
            {
                Data = data,
                PageNo = pageNo,
                PageSize = pageSize,
                PageCount = pageCount
            };

            return Result<AdminEmergencyServicesPaginationResponseModel>.Success(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return Result<AdminEmergencyServicesPaginationResponseModel>.Failure(ex.ToString());
        }
    }
}
