﻿using AcBlog.Data.Models;
using AcBlog.Sdk;
using AcBlog.Sdk.Extensions;
using AcBlog.Sdk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcBlog.Client.Helpers
{
    public static class StatisticHelper
    {
        public static async Task Visited(this IStatisticService service, IClientUrlGenerator urlGenerator, Post data)
        {
            if (service is null)
                return;
            try
            {
                await service.Create(new Statistic
                {
                    Category = "Post",
                    CreationTime = DateTimeOffset.Now,
                    ModificationTime = DateTimeOffset.Now,
                    Uri = urlGenerator.Post(data),
                    Payload = ""
                });
            }
            catch
            {

            }
        }

        public static async Task<int?> Count(this IStatisticService service, IClientUrlGenerator urlGenerator, Post data)
        {
            if (service is null)
                return null;
            try
            {
                return (await service.Query(new Data.Models.Actions.StatisticQueryRequest
                {
                    Category = "Post",
                    Uri = urlGenerator.Post(data),
                    Pagination = new Data.Models.Actions.Pagination
                    {
                        PageSize = 1
                    }
                })).CurrentPage.TotalCount;
            }
            catch
            {
                return null;
            }
        }
    }
}
