﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Deepcove_Trust_Website.Data;
using Microsoft.Extensions.Logging;

namespace Deepcove_Trust_Website.DiscoverDeepCove
{
    [Route("/api/app/activities")]
    public class ActivitiesController : Controller
    {
        private WebsiteDataContext _Db;
        private ILogger<ActivitiesController> _Logger;

        public ActivitiesController(WebsiteDataContext db,  ILogger<ActivitiesController> logger)
        {
            _Db = db;
            _Logger = logger;
        }


        public IActionResult Index()
        {
            try
            {
                return Ok(_Db.Activities.Where(c => c.Active && c.Track.Active)
                .Select(s => new
                {
                    s.Id,
                    updated_at = s.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList());
            }
            catch(Exception ex)
            {
                _Logger.LogError("Error retrieving activities list: {0}", ex.Message);
                _Logger.LogError(ex.StackTrace);
                return BadRequest("Something went wrong, please try again later.");
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult Activity(int id)
        {
            try
            {
                var Activity = _Db.Activities.Where(c => c.Id == id && c.Active & c.Track.Active)
                    .Select(s => new
                    {
                        s.Id,
                        s.Title,
                        s.Description,
                        track_id = s.TrackId,
                        activity_type = (int)s.ActivityType,
                        image_id = s.ImageId,                        

                        coord_x = s.CoordX,
                        coord_y = s.CoordY,
                        qr_code = s.QrCode,
                        Track = s.TrackId,

                        s.Task,
                        activity_images = s.ActivityImages.Select(s1 => s1.ImageId).ToList(),
                        fact_file_id = s.FactFileId,
                        
                        updated_at = s.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).FirstOrDefault();

                if (Activity == null) return NotFound();

                return Ok(Activity);
            }
            catch(Exception ex)
            {
                _Logger.LogError("Error retrieving activity data: {0}", ex.Message);
                _Logger.LogError(ex.StackTrace);
                return BadRequest("Something went wrong, please try again later.");
            }
        }
    }
}