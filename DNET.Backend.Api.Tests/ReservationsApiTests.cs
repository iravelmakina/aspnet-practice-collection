// using System.Net;
// using System.Net.Http.Json;
// using System.Text;
// using Microsoft.AspNetCore.Mvc.Testing;
//
// namespace DNET.Backend.Api.Tests;
//
// public sealed class ReservationsApiTests : BaseApiTests
// {
//     public ReservationsApiTests(WebApplicationFactory<Program> factory) : base(factory)
//     {
//     }   
//     
//     [Fact]
//     public async Task GetReservations_ShouldReturnAllReservations()
//     {
//        await Client.PostAsync("/reservations", new StringContent(
//                """{"clientId":1,"tableId":1,"startTime":"2025-03-01T18:00:00","endTime":"2025-03-01T22:00:00"}""",
//                 Encoding.UTF8,
//                 "application/json"
//             )
//         );
//         
//         var response = await Client.GetAsync("/reservations");
//
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//         
//         var json = await response.Content.ReadAsStringAsync();
//
//         Assert.Equal("""[{"clientId":1,"tableId":1,"startTime":"2025-03-01T18:00:00","endTime":"2025-03-01T22:00:00"}]""", json);
//         
//         await Client.DeleteAsync("/reservations/1");
//     }
//
//     [Fact]
//     public async Task GetReservationById_ShouldReturnReservation_WhenExists()
//     {
//         var newReservation = new Reservation { ClientId = 1, TableId = 1, 
//             StartTime = DateTime.Parse("2025-03-01T18:00:00"), EndTime = DateTime.Parse("2025-03-01T22:00:00")};
//
//        await Client.PostAsJsonAsync("/reservations", newReservation);
//         
//         var response = await Client.GetAsync("/reservations/1");
//
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//
//         var json = await response.Content.ReadAsStringAsync();
//
//         Assert.Equal("""{"clientId":1,"tableId":1,"startTime":"2025-03-01T18:00:00","endTime":"2025-03-01T22:00:00"}""", json);
//
//         await Client.DeleteAsync("/reservations/1");
//     }
//
//     [Fact]
//     public async Task GetReservationById_ShouldReturnNotFound_WhenReservationDoesNotExist()
//     {
//         var response = await Client.GetAsync("/reservations/999");
//
//         Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//     }
//     
//     [Fact]
//     public async Task GetReservationsWithParameters_ShouldReturnAllFilteredReservations()
//     {
//         var newReservation1 = new Reservation { ClientId = 1, TableId = 1, 
//             StartTime = DateTime.Parse("2025-03-01T18:00:00"), EndTime = DateTime.Parse("2025-03-01T22:00:00")};
//         var newReservation2 = new Reservation { ClientId = 2, TableId = 1, 
//             StartTime = DateTime.Parse("2025-03-01T16:00:00"), EndTime = DateTime.Parse("2025-03-01T18:00:00")};
//         
//         await Client.PostAsJsonAsync("/reservations", newReservation1);
//         await Client.PostAsJsonAsync("/reservations", newReservation2);
//         
//         var response = await Client.GetAsync("/reservations?tableId=1&date=2025-03-01");
//
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//         
//         var reservations = await response.Content.ReadFromJsonAsync<List<Reservation>>();
//         Assert.NotNull(reservations);
//         Assert.NotEmpty(reservations);
//         Assert.All(reservations, x =>
//         { 
//             Assert.Equal(1, x.TableId);
//             Assert.Equal(DateTime.Parse("2025-03-01T00:00:00").Date, x.StartTime.Date);
//         });
//         
//         await Client.DeleteAsync("/reservations/1");
//         await Client.DeleteAsync("/reservations/2");
//     }
//     
//     [Fact]
//     public async Task GetReservationsWithParameters_ShouldReturnNotFound_WhenReservationsDoNotExist()
//     {
//         var response = await Client.GetAsync("/reservations?tableId=1&date=2025-03-01");
//
//         Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//     }
//     
//     [Fact]
//     public async Task GetReservationsWithParameters_ShouldReturnBadRequest_WhenParametersAreInvalid()
//     {
//         var response = await Client.GetAsync("/reservations?frjskla=1");
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//     }
//     
//     [Fact]
//     public async Task CreateReservation_ShouldReturnCreated()
//     {
//         var newReservation = new Reservation { ClientId = 1, TableId = 1, 
//             StartTime = DateTime.Parse("2025-03-01T18:00:00"), EndTime = DateTime.Parse("2025-03-01T22:00:00")};
//
//         var response = await Client.PostAsJsonAsync("/reservations", newReservation);
//         
//         Assert.Equal(HttpStatusCode.Created, response.StatusCode);
//
//         var json = await response.Content.ReadAsStringAsync();
//
//         Assert.Equal("""{"clientId":1,"tableId":1,"startTime":"2025-03-01T18:00:00","endTime":"2025-03-01T22:00:00"}""", json);
//
//         await Client.DeleteAsync("/reservations/1");
//     }
//     
//     [Fact]
//     public async Task DeleteReservation_ShouldReturnNoContent_WhenReservationExists()
//     {
//         var newReservation = new Reservation { ClientId = 1, TableId = 1, 
//             StartTime = DateTime.Parse("2025-03-01T18:00:00"), EndTime = DateTime.Parse("2025-03-01T22:00:00")};
//
//         await Client.PostAsJsonAsync("/reservations", newReservation);
//         
//         var response = await Client.DeleteAsync("/reservations/1");
//
//         Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
//     }
//     
//     [Fact]
//     public async Task DeleteReservation_ShouldReturnNotFound_WhenReservationDoesNotExist()
//     {
//         var response = await Client.DeleteAsync("/reservations/999");
//
//         Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//     }
// }
