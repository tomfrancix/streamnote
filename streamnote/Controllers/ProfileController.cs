﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using streamnote.Data;
using streamnote.Mapper;
using streamnote.Models;
using streamnote.Models.Descriptors;

namespace streamnote.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> UserManager;
            private readonly ApplicationDbContext Context;
            private readonly ProfileMapper ProfileMapper;
            private readonly ItemMapper ItemMapper;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ProfileMapper profileMapper, ItemMapper itemMapper)
        {
            UserManager = userManager;
            Context = context;
            ProfileMapper = profileMapper;
            ItemMapper = itemMapper;
        }

        public async Task<IActionResult> View(string username)
        {
            var user = await UserManager.GetUserAsync(User);

            var userInfo = Context.Users.FirstOrDefault(u => u.UserName == username);

            var profile = ProfileMapper.MapDescriptor(userInfo, user.UserName);

            profile.Posts = ItemMapper.MapDescriptors(Context.Items
                .Include(u => u.User)
                .Include(l => l.Likes).ThenInclude(l => l.User)
                .Where(i => i.User.UserName == username)
                .ToList(), user.Id);

            profile.Images = new List<ImageDescriptor>();

            foreach (var post in profile.Posts)
            {
                if (post.Image != null && post.ImageContentType != null)
                {
                    profile.Images.Add(new ImageDescriptor()
                    {
                        UserImage = post.Image,
                        UserImageContentType = post.ImageContentType
                    });
                }
            }

            return View(profile);
        }

        public async Task UpdateConnectionId(string connectionId)
        {
            var user = await UserManager.GetUserAsync(User);

            user.ConnectionId = connectionId;

            Context.Update(user);
            await Context.SaveChangesAsync();
        }
    }
}
