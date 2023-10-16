﻿using Microsoft.AspNetCore.Mvc;
using CabinFever.Models;
using Microsoft.EntityFrameworkCore;
using CabinFever.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinFever.DAL;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Newtonsoft.Json;

namespace CabinFever.Controllers
{
    public class OrderController : Controller
    {
        private readonly ItemDbContext _itemDbContext;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ItemDbContext itemDbContext, ILogger<OrderController> logger)
        {
            _itemDbContext = itemDbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Table()
        {
            List<Order> orders = await _itemDbContext.Orders.ToListAsync();
            return View(orders);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetDateRange(int itemId)
        {
            var dateRanges = _itemDbContext.Orders.Where(order => order.ItemId == itemId && order.FromDate >= DateTime.Today)
                .Select(order => new { order.FromDate, order.ToDate })
                .ToList();

            var dateList = new List<String>();

            foreach (var dateRange in dateRanges)
            {
                for (var date = dateRange.FromDate; date <= dateRange.ToDate; date = date.AddDays(1))
                {
                    var stringDate = date.ToString("yyyy-MM-dd");
                    dateList.Add(stringDate);
                }
            }
            return Json(dateList);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);            // Hent UserId
            order.UserId = userId;                                              // Sett UserId på order

            // Log the order state
            _logger.LogInformation("Order before saving: {@Order}", order);

            // Oppdater ModelState manuelt
            ModelState.Clear();
            TryValidateModel(order);

            // Sjekker om ModelState er gyldig og 'logger' dersom bruker har oppgitt info som ikke oppfyller krav
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogError("Model validation error for {Key}: {ErrorMessage}", state.Key, error.ErrorMessage);
                    }
                }
                return View(order);
            }

            try
            {
                _itemDbContext.Orders.Add(order);           //Legger til ordre i database
                await _itemDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving order: {@Order}", order);     //Log hvis feil oppstår
                throw; 
            }
            return RedirectToAction("Index", "Home");
            //return RedirectToAction("OrderConfirmation", "Reservation", new { orderId = order.OrderId });

        }
        /*
        public IActionResult OrderConfirmation(int orderId)
        {
            // Retrieve the order details from the database based on orderId
            // You can use the orderId to query the database and retrieve the order details
            // For example:
            var order = _itemDbContext.Orders.FirstOrDefault(o => o.OrderId == orderId);

            // Pass the order to the view
            return View(order);
        }
        */
        /*
        private decimal CalculateTotalPrice(Item item, int guests, int numberOfNights)
        {
            decimal pricePerNight = item.PricePerNight;
            decimal cleaningFee = 400 + (pricePerNight * 0.05m);
            decimal serviceFee = pricePerNight * 0.10m;
            decimal taxes = pricePerNight * 0.025m;

            decimal totalPrice = (pricePerNight * numberOfNights) + cleaningFee + serviceFee + taxes;

            return totalPrice;
        }
        */
    }
}
