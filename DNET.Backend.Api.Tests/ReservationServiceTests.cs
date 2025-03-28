using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using DNET.Backend.Api.Requests;
using DNET.Backend.Api.Services;
using DNET.Backend.DataAccess;
using DNET.Backend.DataAccess.Domain;
using Microsoft.Extensions.Options;
using Moq;
using Xunit.Abstractions;

namespace DNET.Backend.Api.Tests;

public sealed class ReservationServiceTests
{
    private TableReservationsDbContext _dbContext;
    private ReservationService _service;
    
    public ReservationServiceTests(ITestOutputHelper testOutputHelper)
    {
        var mockOptionsMonitor = new Mock<IOptionsMonitor<ReservationOptions>>();
        
        mockOptionsMonitor
            .Setup(o => o.CurrentValue)
            .Returns(new ReservationOptions { ReservationLimit = 1 });

        _dbContext = Utils.CreateInMemoryDatabaseContext();
        _service = new ReservationService(mockOptionsMonitor.Object, _dbContext);
    }
    
    [Fact]
    public void TableService_ShouldCreateEmptyReservationsList()
    {
        Assert.Empty(_dbContext.Reservations.ToList());
    }
    
    [Fact]
    public void GetAllReservations_ShouldReturnAllReservations()
    {
        _dbContext.Tables.Add(new TableEntity { Id = 1, Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});
        var reservation = new ReservationEntity
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            StartTime = DateTime.Parse("2025-03-01T18:00:00"),
            EndTime = DateTime.Parse("2025-03-01T22:00:00"),
            ReservationDetail = new ReservationDetailEntity()
            
        };
        _dbContext.Reservations.Add(reservation);
        _dbContext.SaveChanges();

        var resultList = _service.GetAllReservations();

        Assert.NotNull(resultList);
        Assert.Single(resultList);
        Assert.Equal("2025-03-01T18:00:00", resultList[0].StartTime);
        Assert.Equal("2025-03-01T22:00:00", resultList[0].EndTime);
        Assert.Equal(1, resultList[0].TableNumber);
        Assert.Equal("John Doe", resultList[0].ClientName);
        Assert.Equal("Nhoj Eod", resultList[0].HostName);
    }

    [Fact]
    public void GetReservationById_ShouldReturnReservation_WhenExists()
    {
        _dbContext.Tables.Add(new TableEntity { Id = 1, Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});
        var reservation = new ReservationEntity
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            StartTime = DateTime.Parse("2025-03-01T18:00:00"),
            EndTime = DateTime.Parse("2025-03-01T22:00:00"),
            ReservationDetail = new ReservationDetailEntity()
        };
        _dbContext.Reservations.Add(reservation);
        _dbContext.SaveChanges();

        var result = _service.GetReservation(1);

        Assert.NotNull(result);
        Assert.Equal("2025-03-01T18:00:00", result.StartTime);
        Assert.Equal("2025-03-01T22:00:00", result.EndTime);
        Assert.Equal(1, result.TableNumber);
        Assert.Equal("John Doe", result.ClientName);
        Assert.Equal("Nhoj Eod", result.HostName);
    }

    [Fact]
    public void GetReservationById_ShouldReturnNull_WhenReservationDoesNotExist()
    {
        var result = _service.GetReservation(1);

        Assert.Null(result);
    }
    
    [Fact]
    public void GetReservationsWithParameters_ShouldReturnAllFilteredReservations()
    {
        _dbContext.Tables.Add(new TableEntity { Id = 1, Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Tables.Add(new TableEntity { Id = 2, Number = 2, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 2, Name = "Nhoj Eod 2"} });
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});

        for (int i = 1; i < 6; i++)
        {
            var reservation = new ReservationEntity
            {
                Id = i,
                ClientId = 1,
                TableId = (i % 2) + 1,
                StartTime = DateTime.Parse("2025-03-01T14:00:00").AddHours(2 * i),
                EndTime = DateTime.Parse("2025-03-01T16:00:00").AddHours(2 * i),
                ReservationDetail = new ReservationDetailEntity
                {
                    ReservationType = (ReservationDetailEntity.ReservationTypeEnum)(i % 2)
                }
            };
            _dbContext.Reservations.Add(reservation);
        }
        _dbContext.SaveChanges();

        var resultList = _service.GetAllReservations(1, 2, DateTime.Parse("2025-03-01T12:00:00"), "Birthday");

        Assert.NotNull(resultList);
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, x =>
        { 
            Assert.Equal("John Doe", x.ClientName);
            Assert.Equal(2, x.TableNumber);
            Assert.Equal(DateTime.Parse("2025-03-01").Date, DateTime.Parse(x.StartTime).Date);
            Assert.Equal("Birthday", x.ReservationType);
            Assert.Equal("Nhoj Eod 2", x.HostName);
        });
    }
    
    [Fact]
    public void GetReservationsWithParameters_ShouldReturnEmpty_WhenReservationsDoesNotExist()
    {
        var resultList = _service.GetAllReservations(1, 1, DateTime.Parse("2025-03-01"), "Meeting");

        Assert.Empty(resultList);
    }
    
    [Fact]
    public void CreateReservation_ShouldReturnCreatedReservationKV_WhenReservationLimitIsNotExceeded()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});
        var request = new CreateUpdateReservationRequest
        {
            ClientId = 1,
            TableNumber = 1,
            StartTime = "2025-03-01T18:00:00",
            EndTime = "2025-03-01T22:00:00",
            ReservationType = "Birthday",
            SpecialRequests = "Cake at face"
        };
        _dbContext.SaveChanges();
        
        var result = _service.AddReservation(request);
        _dbContext.SaveChanges();
        
        Assert.NotNull(result);
        Assert.Equal("2025-03-01T18:00:00", result.Item2.StartTime);
        Assert.Equal("2025-03-01T22:00:00", result.Item2.EndTime);
        Assert.Equal("Birthday", result.Item2.ReservationType);
        Assert.Equal("Cake at face", result.Item2.SpecialRequests);
        Assert.Equal(1, result.Item2.TableNumber);
        Assert.Equal("John Doe", result.Item2.ClientName);
        Assert.Equal("Nhoj Eod", result.Item2.HostName);
    }
    
    [Fact]
    public void CreateReservation_ShouldReturnNull_WhenReservationLimitIsExceeded()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});
        var request = new CreateUpdateReservationRequest
        {
            ClientId = 1,
            TableNumber = 1,
            StartTime = "2025-03-01T18:00:00",
            EndTime = "2025-03-01T22:00:00",
            ReservationType = "Birthday",
            SpecialRequests = "Cake at face"
        };
        _dbContext.SaveChanges();

        _service.AddReservation(request);
        
        // New reservation for same client
        request.StartTime = "2025-03-02T18:00:00";
        request.EndTime = "2025-03-02T22:00:00";
        
        var result = _service.AddReservation(request);
        
        Assert.Null(result);
    }

    [Fact]
    public void UpdateReservation_ShouldReturnUpdatedReservation_WhenParametersAreCorrect()
    {
        _dbContext.Tables.Add(new TableEntity { Id = 1, Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Tables.Add(new TableEntity { Id = 2, Number = 2, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 2, Name = "Nhoj Eod 2"} });
        
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});
        _dbContext.Clients.Add(new ClientEntity { Id = 2, Name = "John Doe The Second", Phone = "+380987345692"});
        
        var reservation = new ReservationEntity
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            StartTime = DateTime.Parse("2025-03-01T18:00:00"),
            EndTime = DateTime.Parse("2025-03-01T22:00:00"),
            ReservationDetail = new ReservationDetailEntity()
        };
        _dbContext.Reservations.Add(reservation);
        _dbContext.SaveChanges();

        var result = _service.UpdateReservation(1, new CreateUpdateReservationRequest
        {
            ClientId = 2,
            TableNumber = 2,
            StartTime = "2025-03-01T19:00:00",
            EndTime = "2025-03-01T23:00:00",
            ReservationType = "SpecialEvent",
            SpecialRequests = "Serve beforehand"
        });
        
        Assert.Equal("John Doe The Second", result.ClientName);
        Assert.Equal(2, result.TableNumber);
        Assert.Equal("2025-03-01T19:00:00", result.StartTime);  // UTC
        Assert.Equal("2025-03-01T23:00:00", result.EndTime);
        Assert.Equal("SpecialEvent", result.ReservationType);
        Assert.Equal("Serve beforehand", result.SpecialRequests);
    }
    
    [Fact]
    public void UpdateReservation_ShouldThrowBadRequest_WhenNewTableAndClientDoNotExist()
    {
        _dbContext.Tables.Add(new TableEntity { Id = 1, Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});
        
        var reservation = new ReservationEntity
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            StartTime = DateTime.Parse("2025-03-01T18:00:00"),
            EndTime = DateTime.Parse("2025-03-01T22:00:00"),
            ReservationDetail = new ReservationDetailEntity()
        };
        _dbContext.Reservations.Add(reservation);
        _dbContext.SaveChanges();

        Assert.Throws<BadRequestException>(() => _service.UpdateReservation(1, new CreateUpdateReservationRequest
        {
            ClientId = 2,
            TableNumber = 2,
            StartTime = "2025-03-01T19:00:00",
            EndTime = "2025-03-01T23:00:00",
            ReservationType = "SpecialEvent",
            SpecialRequests = "Serve beforehand"
        }));
    }
    
    [Fact]
    public void UpdateReservation_ShouldThrowBadRequest_WhenConflictingReservationExists()
    {
        _dbContext.Tables.Add(new TableEntity { Id = 1, Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});
        
        var reservation1 = new ReservationEntity
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            StartTime = DateTime.Parse("2025-03-01T18:00:00"),
            EndTime = DateTime.Parse("2025-03-01T22:00:00"),
            ReservationDetail = new ReservationDetailEntity()
        };
        var reservation2 = new ReservationEntity
        {
            Id = 2,
            ClientId = 1,
            TableId = 1,
            StartTime = DateTime.Parse("2025-03-01T16:00:00"),
            EndTime = DateTime.Parse("2025-03-01T17:30:00"),
            ReservationDetail = new ReservationDetailEntity()
        };
        _dbContext.Reservations.Add(reservation1);
        _dbContext.Reservations.Add(reservation2);
        _dbContext.SaveChanges();

        Assert.Throws<BadRequestException>(() => _service.UpdateReservation(1, new CreateUpdateReservationRequest
        {
            StartTime = "2025-03-01T17:00:00",
            EndTime = "2025-03-01T18:00:00"
        }));
    }
    
    [Fact]
    public void DeleteReservation_ShouldReturnTrue_WhenReservationExists()
    {
        _dbContext.Tables.Add(new TableEntity { Id = 1, Number = 1, Capacity = 4, LocationId = 1, 
            Host = new HostEntity { Id = 1, Name = "Nhoj Eod"} });
        _dbContext.Clients.Add(new ClientEntity { Id = 1, Name = "John Doe", Phone = "+380987345692"});
        var reservation = new ReservationEntity
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            StartTime = DateTime.Parse("2025-03-01T18:00:00"),
            EndTime = DateTime.Parse("2025-03-01T22:00:00")
        };
        _dbContext.Reservations.Add(reservation);
        _dbContext.SaveChanges();
        
        var result = _service.DeleteReservation(1);
        
        Assert.True(result);
        var deletedReservation = _dbContext.Reservations.Find(reservation.Id);
        Assert.Null(deletedReservation); 
    }
    
    [Fact]
    public void DeleteReservation_ShouldReturnNotFalse_WhenReservationDoesNotExist()
    {
        var result = _service.DeleteReservation(1);
        
        Assert.False(result);
    }
}