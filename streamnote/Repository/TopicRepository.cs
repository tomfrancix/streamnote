﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using streamnote.Data;
using streamnote.Models;
using streamnote.Repository.Interface;

namespace streamnote.Repository
{
    /// <summary>
    /// Repository class for item database calls.
    /// </summary>
    public class TopicRepository : ITopicRepository
    {
        private readonly ApplicationDbContext Context;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        public TopicRepository(ApplicationDbContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Get all the items.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Topic> QueryAllTopics()
        {
            return Context.Topics
                .Include(t => t.Users)
                .Where(t => t.ItemCount > 0);
        }

        public Topic QueryExistingTopic(string name)
        {
            return Context.Topics.FirstOrDefault(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task IncrementItemCount(Topic topic)
        {
            topic.ItemCount++;
            Context.Update(topic);
            await Context.SaveChangesAsync();
        }

        public async Task<Topic> CreateTopic(Topic topic)
        {
            Context.Add(topic);
            await Context.SaveChangesAsync();
            return QueryExistingTopic(topic.Name);
        }
    }
}
