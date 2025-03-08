using DNET.Backend.Api.Models;
using DNET.Backend.Api.Services;

namespace DNET.Backend.Api.Tests;

public class ReservationServiceTests
{
    [Fact]
    public void ReservationService_ShouldCreateEmptyReservationsList()
    {
        var reservationService = new ReservationService();
        Assert.Empty(reservationService.GetAllReservations());
    }
    
    [Fact]
    public void GetAllReservations_ShouldReturnAllReservations()
    {
        var reservationService = new ReservationService();
        var reservation = new Reservation(1, 1, "2025-03-01T18:00:00", "2025-03-01T22:00:00");
        reservationService.AddReservation(reservation);

        var resultList = reservationService.GetAllReservations();

        Assert.NotNull(resultList);
        Assert.Single(resultList);
        Assert.Equal(reservation, resultList[0]);
    }

    [Fact]
    public void GetReservationById_ShouldReturnReservation_WhenExists()
    {
        var reservationService = new ReservationService();
        var reservation = new Reservation(1, 1, "2025-03-01T18:00:00", "2025-03-01T22:00:00");
        reservationService.AddReservation(reservation);

        var result = reservationService.GetReservation(1);

        Assert.NotNull(result);
        Assert.Equal(reservation, result);
    }

    [Fact]
    public void GetReservationById_ShouldReturnNull_WhenReservationDoesNotExist()
    {
        var reservationService = new ReservationService();
        
        var result = reservationService.GetReservation(1);

        Assert.Null(result);
    }
    
    [Fact]
    public void GetReservationsWithParameters_ShouldReturnAllFilteredReservations()
    {
        var reservationService = new ReservationService();
        var reservation1 = new Reservation(1, 1, "2025-03-01T18:00:00", "2025-03-01T22:00:00");
        var reservation2 = new Reservation(2, 1, "2025-03-01T16:00:00", "2025-03-01T18:00:00");
        reservationService.AddReservation(reservation1);
        reservationService.AddReservation(reservation2);

        var resultList = reservationService.GetAllReservations(1, DateTime.Parse("2025-03-01"));

        Assert.NotNull(resultList);
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, x =>
        { 
            Assert.Equal(1, x.TableId);
            Assert.Equal(DateTime.Parse("2025-03-01T00:00:00").Date, x.StartTime.Date);
        });
    }
    
    [Fact]
    public void GetReservationsWithParameters_ShouldReturnEmpty_WhenReservationsDoNotExist()
    {
        var reservationService = new ReservationService();
        var resultList = reservationService.GetAllReservations(1, DateTime.Parse("2025-03-01"));

        Assert.Empty(resultList);
    }
    
    [Fact]
    public void CreateReservation_ShouldReturnCreatedReservationKV()
    {
        var reservationService = new ReservationService();
        var newReservation = new Reservation(1, 1, "2025-03-01T18:00:00", "2025-03-01T22:00:00");
        
        var newReservationPair = reservationService.AddReservation(newReservation);
        
        Assert.NotNull(newReservationPair);
        Assert.Equal(newReservation, newReservationPair.Item2);
        Assert.Single(reservationService.GetAllReservations());
    }
    
    [Fact]
    public void DeleteReservation_ShouldReturnTrue_WhenReservationExists()
    {
        var reservationService = new ReservationService();
        var reservation = new Reservation(1, 1, "2025-03-01T18:00:00", "2025-03-01T22:00:00");
        reservationService.AddReservation(reservation);
        
        var result = reservationService.DeleteReservation(1);
        
        Assert.True(result);
        Assert.Empty(reservationService.GetAllReservations());
    }
    
    [Fact]
    public void DeleteReservation_ShouldReturnNotFalse_WhenReservationDoesNotExist()
    {
        var reservationService = new ReservationService();
        
        var result = reservationService.DeleteReservation(1);
        
        Assert.False(result);
    }
}