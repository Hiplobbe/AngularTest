﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AngularTest.Controllers
{
    public class MainController : Controller
    {
        public ActionResult Game()
        {
            return View("~/Views/Game.cshtml");
        }
    }
}