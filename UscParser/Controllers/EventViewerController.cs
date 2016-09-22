using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UscParser.Controllers
{
	[RoutePrefix("EventViewer")]
    public class EventViewerController : Controller
    {
		[Route("{id?}")]
		public ActionResult Index(string id)
        {
			var results = EventInfo.Load(id);

			return View(results);
        }
    }
}