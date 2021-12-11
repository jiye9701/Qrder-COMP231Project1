using Amazon.DynamoDBv2;
using Amazon.Runtime;
using AutoMapper;
using c231_qrder.Models;
using c231_qrder.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace c231_qrder.Controllers
{
    [ApiController]
    [Route("api/")]
    public class OrderController : ControllerBase
    {
        private readonly IOrdersService ordersService;

        public OrderController(
            IAmazonDynamoDB dynamoDBClient,
            IMapper mapper
            )
        {
            ordersService = new OrdersService(dynamoDBClient, mapper);
        }

        // GET: api/restaurant/5/orders
        [HttpGet("restaurant/{id}/orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllTables(string id, [FromQuery(Name = "mode")] string? getMode)
        {
            IEnumerable<OrderDto> resultOrderDtos = new List<OrderDto>();
            try
            {
                resultOrderDtos = await ordersService.GetAllByRestaurantIdAsync(id, getMode);
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

            return Ok(resultOrderDtos);
        }

        // POST: api/restaurant/5/order
        [HttpPost("restaurant/{id}/order")]
        public async Task<IActionResult> PostTable(
            string id,
            OrderCreateDto orderCreateDto)
        {
            // Returns errors
            if (orderCreateDto is null || id != orderCreateDto.RestaurantId)
            {
                return BadRequest();
            }
            if (orderCreateDto.OrderedItems.Count == 0)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await ordersService.AddAsync(id, orderCreateDto);
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

        // PUT: api/restaurant/5/order
        [HttpPut("restaurant/{id}/order")]
        public async Task<IActionResult> PutTable(string id, OrderDto input)
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
                await ordersService.SaveAsync(id, input);
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

        // DELETE: api/restaurant/5/order?oid=5&mode=delete
        [HttpDelete("restaurant/{id}/order")]
        public async Task<IActionResult> DeleteOrder(
            string id,
            [FromQuery(Name = "mode")] string? deleteMode,
            OrderDeleteDto input
            )
        {
            try
            {
                if (deleteMode is not null)
                {
                    if (deleteMode != "delete")
                    {
                        return BadRequest();
                    }
                    else
                    {
                        // mode is not null && mode == "delete"
                        await ordersService.RemoveAsync(id, input);
                    }
                }
                else
                {
                    // mode is null
                    await ordersService.ArchiveAsync(id, input);
                }
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
