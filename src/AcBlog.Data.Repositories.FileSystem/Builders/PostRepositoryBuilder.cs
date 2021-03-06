﻿using AcBlog.Data.Models;
using AcBlog.Data.Repositories.FileSystem.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;
using AcBlog.Data.Models.Actions;
using AcBlog.Data.Protections;
using System.Security.Cryptography;
using AcBlog.Data.Documents;
using Microsoft.Extensions.FileProviders;
using AcBlog.Data.Extensions;

namespace AcBlog.Data.Repositories.FileSystem.Builders
{
    public class PostRepositoryBuilder : RecoderRepositoryBuilderBase<Post, string>
    {
        public PostRepositoryBuilder(string rootPath) : base(rootPath)
        {
        }

        async Task BuildIndexType(IList<Post> data)
        {
            {
                PagingProvider<string> paging = new PagingProvider<string>(Paths.GetArticleRoot(RootPath));

                await paging.Build(data.Where(
                    x => x.Type == PostType.Article).ToList().Select(x => x.Id).ToArray(),
                    PagingConfig).ConfigureAwait(false);
            }

            {
                PagingProvider<string> paging = new PagingProvider<string>(Paths.GetSlidesRoot(RootPath));

                await paging.Build(data.Where(
                    x => x.Type == PostType.Slides).ToList().Select(x => x.Id).ToArray(),
                    PagingConfig).ConfigureAwait(false);
            }

            {
                PagingProvider<string> paging = new PagingProvider<string>(Paths.GetNoteRoot(RootPath));

                await paging.Build(data.Where(
                    x => x.Type == PostType.Note).ToList().Select(x => x.Id).ToArray(),
                    PagingConfig).ConfigureAwait(false);
            }
        }

        async Task BuildIndexKeyword(IList<Post> data)
        {
            FSStaticBuilder.EnsureDirectoryEmpty(Paths.GetKeywordRoot(RootPath));

            var (collection, map) = KeywordCollectionBuilder.BuildFromPosts(data);

            foreach (var v in collection.Items)
            {
                PagingProvider<string> paging = new PagingProvider<string>(Paths.GetKeywordRoot(RootPath, v));

                await paging.Build(map[v.OneName()].Select(x => x.Id).ToArray(),
                    PagingConfig).ConfigureAwait(false);
            }

            using var st = FSStaticBuilder.GetFileRewriteStream(Paths.GetKeywordMetadata(RootPath));
            await JsonSerializer.SerializeAsync(st, collection).ConfigureAwait(false);
        }

        async Task BuildIndexCategory(IList<Post> data)
        {
            FSStaticBuilder.EnsureDirectoryEmpty(Paths.GetCategoryRoot(RootPath));

            var (tree, map) = CategoryTreeBuilder.BuildFromPosts(data);

            Queue<CategoryTree.Node> q = new Queue<CategoryTree.Node>();
            foreach (var v in tree.Root.Children.Values)
                q.Enqueue(v);

            while (q.Count > 0)
            {
                var node = q.Dequeue();

                PagingProvider<string> paging = new PagingProvider<string>(Paths.GetCategoryRoot(RootPath, node.Category));

                await paging.Build(map[node].Select(x => x.Id).ToArray(),
                    PagingConfig).ConfigureAwait(false);

                foreach (var v in node.Children.Values)
                    q.Enqueue(v);
            }

            using var st = FSStaticBuilder.GetFileRewriteStream(Paths.GetCategoryMetadata(RootPath));
            await JsonSerializer.SerializeAsync(st, tree).ConfigureAwait(false);
        }

        public override async Task Build(IList<Post> data)
        {
            data = (from x in data orderby x.CreationTime descending select x).ToArray();
            await base.Build(data).ConfigureAwait(false);
            await BuildIndexType(data).ConfigureAwait(false);
            await BuildIndexKeyword(data).ConfigureAwait(false);
            await BuildIndexCategory(data).ConfigureAwait(false);
        }
    }
}
