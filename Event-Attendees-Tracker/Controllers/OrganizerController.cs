﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Diagnostics;
using System.Web.Mvc;

using Event_Attendees_Tracker.Filters;
using Event_Attendees_Tracker.Middlewares;
using Event_Attendees_Tracker.Modals;

namespace Event_Attendees_Tracker.Controllers
{
    [Authorize(Roles ="Organizer")]
    public class OrganizerController : Controller
    {
        RestClient client = new RestClient("https://localhost:44360/");

        //GET: /Organizer/Organizer        
        public ActionResult Dashboard()
        {
            return View();
        }

        //GET: Organizer/CreateEvent        
        public ActionResult CreateEvent()
        {
            ViewBag.Readonly = false;
            return View();
        }

        //POST: /Organizer/CreateEvent
        [HttpPost]
        [Authorize(Roles = "Organizer")]        
        public RedirectToRouteResult CreateEvent(EventModel responseEventModel)
        {
            var excelFilePath = "";
            var imageFilePath = "";

            //Save Excel File
            if (responseEventModel.excelFile.ContentLength > 0 && responseEventModel.excelFile.ContentType.Contains("spreadsheetml"))
            {
                //Excel File

                excelFilePath = System.Web.HttpContext.Current.Server.MapPath($@"~/StudentExcel/{DateTime.Now.ToFileTime()}{responseEventModel.excelFile.FileName}");
                responseEventModel.excelFile.SaveAs(excelFilePath);
            }

            //Save Poster Image
            if (responseEventModel.posterImage.ContentLength > 0 && responseEventModel.posterImage.ContentType.Contains("image"))
            {
                //Poster Image File

                imageFilePath = System.Web.HttpContext.Current.Server.MapPath($@"~/PosterImage/{DateTime.Now.ToFileTime()}{responseEventModel.posterImage.FileName}");
                responseEventModel.posterImage.SaveAs(imageFilePath);
            }

            //Get the Datatable After Parsing
            var parsedDataTable = new ParseExcel().InsertTblRegisteredStudents(excelFilePath);


            //To Delete the file
            if (System.IO.File.Exists(excelFilePath))
            {
                System.IO.File.Delete(excelFilePath);
            }


            //Request Config
            var request = new RestRequest("api/User/CreateEvent");
            request.Method = Method.POST;

            //Adding JSON Body

            //TODO:
            //Add Volunteer Reference

            var requestedData = new
            {
                Name = responseEventModel.name,
                Venue = responseEventModel.venue,
                Description = responseEventModel.description,
                EventDate = Convert.ToDateTime(responseEventModel.eventDate),
                StartTime = responseEventModel.startTime,
                EndTime = responseEventModel.endTime,
                PosterImagePath = imageFilePath,
                AttendeesDataTable = parsedDataTable
            };

            request.AddJsonBody(JsonConvert.SerializeObject(requestedData, Formatting.Indented));

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                Debug.Print(response.Content);

            }
            return RedirectToAction("CreateEvent");

        }

        //GET: /Organizer/ModifyEvent
        [HttpGet]
        public ActionResult ModifyEvent()
        {
            return View();
        }
        //GET: /Organizer/Reports
        public ActionResult Reports()
        {

            return View("OrganizerReports");
        }
    }
}