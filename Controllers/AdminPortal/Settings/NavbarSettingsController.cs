﻿using Deepcove_Trust_Website.Data;
using Deepcove_Trust_Website.Helpers;
using Deepcove_Trust_Website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Deepcove_Trust_Website.Helpers.Utils;

namespace Deepcove_Trust_Website.Controllers.AdminPortal.Settings
{
    [Authorize]
    [Area("admin-portal,web")]
    [Route("/admin/settings/navbar")]
    public class NavbarSettingsController : Controller
    {
        private readonly WebsiteDataContext _Db;
        private readonly ILogger<NavbarSettingsController> _Logger;

        public NavbarSettingsController(WebsiteDataContext db, ILogger<NavbarSettingsController> logger)
        {
            _Db = db;
            _Logger = logger;
        }

        /// <summary>
        /// Returns a JSON representation of the navbar links, for consumption by the CMS
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                List<NavItem> navItems = await _Db.NavItems
                    .Include(ni => ni.Page)
                    .OrderBy(n => n.Section)
                        .ThenBy(n => n.OrderIndex)
                    .ToListAsync();

                return Ok(navItems.Select(item => new {
                    item.Id,
                    text = item.Text ?? item.Page?.Name,
                    section = item.Section,
                }));                

            }
            catch (Exception ex)
            {
                _Logger.LogError("Error retrieving navbar details", ex.Message);
                _Logger.LogError(ex.StackTrace);
                return BadRequest(new ResponseHelper("Something went wrong, please try again later.", ex.Message));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            try
            {
                NavItem item = await _Db.NavItems.FindAsync(id);

                if (item == null) return NotFound(new ResponseHelper("Something went wrong, please try again later"));

                await _Db.Entry(item).Reference(i => i.Page).LoadAsync();
                await _Db.Entry(item).Collection(i => i.NavItemPages).LoadAsync();

                foreach (NavItemPage nip in item.NavItemPages)
                    await _Db.Entry(nip).Reference(n => n.Page).LoadAsync();

                return Ok(new {
                    item.Id,
                    item.Text,
                    item.Url,
                    pageId = item.Page?.Id,
                    pageName = item.Page?.Name,
                    children = item.NavItemPages.Count > 0 ? item.NavItemPages.OrderBy((nip) => nip.OrderIndex).Select(nip => new
                    {
                        nip.Text,
                        nip.Url,
                        pageId = nip.Page?.Id,
                        pageName = nip.Page?.Name,
                    }) : null
                });
            }
            catch(Exception ex)
            {
                _Logger.LogError("Error retrieving navbar item details", ex.Message);
                _Logger.LogError(ex.StackTrace);
                return BadRequest(new ResponseHelper("Something went wrong, please try again later.", ex.Message));
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddItem(IFormCollection request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Str("navitem")))
                    return BadRequest(new ResponseHelper("Something went wrong, please try again later", "No nav-item data was sent"));

                NavItem newItem = JsonConvert.DeserializeObject<NavItem>(request.Str("navitem"));

                // Give an order index that places this at the end of the list, for its section
                newItem.OrderIndex = await _Db.NavItems.Where(n => n.Section == newItem.Section).DefaultIfEmpty().MaxAsync(m => m.OrderIndex) + 1;

                await _Db.AddAsync(newItem);
                
                // Add new dropdowns, if any
                if (newItem.NavItemPages != null)
                {
                    await _Db.AddRangeAsync(newItem.NavItemPages);
                }

                await _Db.SaveChangesAsync();

                return Ok(newItem.Id);
            }
            catch(Exception ex)
            {
                _Logger.LogError("Error creating new navlink", ex.Message);
                _Logger.LogError(ex.StackTrace);
                return BadRequest(new ResponseHelper("Something went wrong, please try again later.", ex.Message));
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditItem(IFormCollection request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Str("navitem")))
                    return BadRequest(new ResponseHelper("Something went wrong, please try again later", "No nav-item data was sent"));                

                NavItem updatedItem = JsonConvert.DeserializeObject<NavItem>(request.Str("navitem"));

                // If ID is zero, this is a new item, use the AddItem method
                if (updatedItem.Id == 0) return await AddItem(request);

                // Check whether an existing navitem has that id
                NavItem originalItem = await _Db.NavItems.AsNoTracking().FirstOrDefaultAsync((item) => item.Id == updatedItem.Id);

                if (originalItem == null) return NotFound(new ResponseHelper("Something went wrong, please try again later."));

                // Delete any old drop down links for the nav item
                List<NavItemPage> dropdowns = await _Db.NavItemPages.Where(n => n.NavItemId == updatedItem.Id).ToListAsync();
                _Db.RemoveRange(dropdowns);
                await _Db.SaveChangesAsync();

                // Mark item as updated
                _Db.Update(updatedItem);
                _Db.Entry(updatedItem).Property(i => i.OrderIndex).IsModified = false;
                _Db.Entry(updatedItem).Property(i => i.Section).IsModified = false;

                // Add new dropdowns, if any
                if (updatedItem.NavItemPages != null)
                {
                    // Add order index to items before saving 
                    for (int i = 0; i < updatedItem.NavItemPages.Count; i++)
                        updatedItem.NavItemPages[i].OrderIndex = i;

                    _Db.AddRange(updatedItem.NavItemPages);
                }

                // Write changes to database
                await _Db.SaveChangesAsync();

                return Ok(updatedItem.Id);
            }
            catch(Exception ex)
            {
                _Logger.LogError("Error updating navlink", ex.Message);
                _Logger.LogError(ex.StackTrace);
                return BadRequest(new ResponseHelper("Something went wrong, please try again later.", ex.Message));
            }
        }

        [HttpPatch]
        public async Task<IActionResult> ReorderItems(IFormCollection request, string section)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Str("navitems")))
                    return BadRequest(new ResponseHelper("Something went wrong, please try again later", "No nav-item data was sent"));

                if (!Enum.TryParse(section, true, out Section navSection))
                {
                    return BadRequest(new ResponseHelper("Something went wrong, please try again later", "Must define section as 'main' or 'education'"));
                }

                List<int> idOrder = JsonConvert.DeserializeObject<List<int>>(request.Str("navitems"));

                for (int orderIndex = 0; orderIndex < idOrder.Count; orderIndex++)
                {
                    NavItem item = await _Db.NavItems.FindAsync(idOrder[orderIndex]);
                    item.OrderIndex = orderIndex;
                    item.Section = navSection;
                }

                await _Db.SaveChangesAsync();

                return Ok();
            }
            catch(Exception ex)
            {
                _Logger.LogError("Error re-ordering navlinks", ex.Message);
                _Logger.LogError(ex.StackTrace);
                return BadRequest(new ResponseHelper("Something went wrong, please try again later.", ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                NavItem itemToDelete = await _Db.NavItems.FindAsync(id);                

                if (itemToDelete == null)
                    return NotFound(new ResponseHelper("Something went wrong, please try again later"));

                await _Db.Entry(itemToDelete).Collection(c => c.NavItemPages).LoadAsync();
                _Db.Remove(itemToDelete);

                if (itemToDelete.NavItemPages != null)
                    _Db.RemoveRange(itemToDelete.NavItemPages);

                await _Db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _Logger.LogError("Error deleting navlink", ex.Message);
                _Logger.LogError(ex.StackTrace);
                return BadRequest(new ResponseHelper("Something went wrong, please try again later.", ex.Message));
            }
        }
    }
}
