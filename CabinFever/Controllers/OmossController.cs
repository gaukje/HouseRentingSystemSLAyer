﻿using Microsoft.AspNetCore.Mvc;

namespace CabinFever.Controllers
{
    public class MinSideController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
