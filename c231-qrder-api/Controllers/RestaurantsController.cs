using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using AutoMapper;
using c231_qrder.Models;
using c231_qrder.Services;
using Microsoft.AspNetCore.Mvc;

namespace c231_qrder.Controllers
{
    [ApiController]
    [Route("api/")]
    public class RestaurantsController : ControllerBase
    {
        private readonly IRestaurantsService restaurantsService;

        public RestaurantsController(
            IAmazonDynamoDB dynamoDBClient,
            IMapper mapper
            )
        {
            restaurantsService = new RestaurantsService(dynamoDBClient, mapper);
        }

        // GET: api/restaurants
        [HttpGet("restaurants")]
        public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetAllRestaurants()
        {
            IEnumerable<RestaurantDto> resultRestaurantDtos = new List<RestaurantDto>();
            try
            {
                resultRestaurantDtos = await restaurantsService.GetAllAsync();
            }
            catch (AmazonServiceException ase)
            {
                return StatusCode(500, $"{ase.Message}");
            }
            catch
            {
                return StatusCode(500, "Something went wrong while processing the request in the server.");
            }

            return Ok(resultRestaurantDtos);
        }

        // GET: api/restaurant/5
        [HttpGet("restaurant/{id}")]
        public async Task<ActionResult<RestaurantDto>> GetRestaurant(string id)
        {
            RestaurantDto result = new RestaurantDto();
            try
            {
                result = await restaurantsService.GetByIdAsync(id);
            }
            catch (DataException)
            {
                return NotFound();
            }
            catch (AmazonServiceException)
            {
                return StatusCode(500, "A problem happend while processing the request from the remote server.");
            }
            catch
            {
                return StatusCode(500, "Something went wrong while processing the request in the server.");
            }

            return Ok(result);
        }

        // POST: api/restaurant
        [HttpPost("restaurant")]
        public async Task<ActionResult<RestaurantDto>> PostRestaurant(RestaurantCreateDto input)
        {
            // Returns errors
            if (input is null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultRestaurantDto = new RestaurantDto();
            try
            {
                resultRestaurantDto = await restaurantsService.AddAsync(input);
            }
            catch (DataException)
            {
                return NotFound();
            }
            catch (AmazonServiceException)
            {
                return StatusCode(500, "A problem happend while processing the request from the remote server.");
            }
            catch
            {
                return StatusCode(500, "Something went wrong while processing the request in the server.");
            }

            return Ok(resultRestaurantDto);
        }

        // PUT: api/restaurant/5
        [HttpPut("restaurant")]
        public async Task<IActionResult> PutRestaurant(RestaurantDto input)
        {
            // Returns errors
            if (input is null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await restaurantsService.SaveAsync(input);
            }
            catch (DataException)
            {
                return NotFound();
            }
            catch (AmazonServiceException)
            {
                return StatusCode(500, "A problem happend while processing the request from the remote server.");
            }
            catch
            {
                return StatusCode(500, "Something went wrong while processing the request in the server.");
            }

            return NoContent();
        }

        // DELETE: api/restaurant/5
        [HttpDelete("restaurant/{id}")]
        public async Task<IActionResult> DeleteRestaurant(string id)
        {
            try
            {
                await restaurantsService.RemoveAsync(id);
            }
            catch (DataException)
            {
                return NotFound();
            }
            catch (AmazonServiceException)
            {
                return StatusCode(500, "A problem happend while processing the request from the remote server.");
            }
            catch
            {
                return StatusCode(500, "Something went wrong while processing the request in the server.");
            }

            return NoContent();
        }
    }
}
