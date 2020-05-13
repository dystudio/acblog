﻿using AcBlog.Data.Models;
using AcBlog.Data.Models.Actions;
using AcBlog.Data.Repositories.SQLServer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcBlog.Data.Repositories.SQLServer
{
    public class CategoryRepository : ICategoryRepository
    {
        public CategoryRepository(DataContext data)
        {
            Data = data;
        }

        DataContext Data { get; set; }

        public RepositoryAccessContext Context { get; set; }

        public async Task<IEnumerable<string>> All() => await Data.Categories.Select(x => x.Id).ToArrayAsync();

        public Task<bool> CanRead() => Task.FromResult(true);

        public Task<bool> CanWrite() => Task.FromResult(true);

        public async Task<string> Create(Category value)
        {
            if (string.IsNullOrWhiteSpace(value.Id))
                value.Id = Guid.NewGuid().ToString();
            Data.Categories.Add(value);
            await Data.SaveChangesAsync();
            return value.Id;
        }

        public async Task<bool> Delete(string id)
        {
            var item = await Data.Categories.FindAsync(id);
            if (item == null)
                return false;
            Data.Categories.Remove(item);
            await Data.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Exists(string id)
        {
            var item = await Data.Categories.FindAsync(id);
            return item != null;
        }

        public async Task<Category> Get(string id)
        {
            var item = await Data.Categories.FindAsync(id);
            return item;
        }

        public async Task<QueryResponse<string>> Query(CategoryQueryRequest query)
        {
            var qr = Data.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Name))
                qr = qr.Where(x => x.Name.Contains(query.Name));

            Pagination pagination = new Pagination
            {
                TotalCount = await qr.CountAsync(),
            };

            if (query.Pagination != null)
            {
                qr = qr.Skip(query.Pagination.Offset).Take(query.Pagination.CountPerPage);
                pagination.PageNumber = query.Pagination.PageNumber;
                pagination.CountPerPage = query.Pagination.CountPerPage;
            }
            else
            {
                pagination.PageNumber = 0;
                pagination.CountPerPage = pagination.TotalCount;
            }

            return new QueryResponse<string>(await qr.Select(x => x.Id).ToArrayAsync(), pagination);
        }

        public async Task<bool> Update(Category value)
        {
            Data.Categories.Update(value);
            await Data.SaveChangesAsync();
            return true;
        }
    }
}
