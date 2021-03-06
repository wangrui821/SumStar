﻿using System.Collections.Generic;
using System.Linq;

using SumStar.DataAccess;
using SumStar.Models;
using SumStar.Models.ViewModels;

namespace SumStar.Services
{
	public class CategoryService
	{
		private readonly ApplicationDbContext _dbContext;

		public CategoryService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// 获取下级子栏目的树节点。
		/// </summary>
		/// <param name="categoryId">栏目标识。</param>
		/// <param name="archor">链接锚点。</param>
		/// <returns>下级子栏目的树节点。</returns>
		public IList<BootstrapTreeNode> GetBootstrapTreeNodes(int categoryId, string archor)
		{
			var query = from category in _dbContext.Categories
						where category.ParentId == categoryId
						orderby category.DisplayOrder descending, category.CreateTime descending
						select new BootstrapTreeNode
						{
							Text = category.Name,
							Href = "/Contents/List?categoryId=" + category.Id + "#" + archor,
							Tags = new[] { category.Id.ToString() }
						};
			IList<BootstrapTreeNode> nodes = query.ToList();
			foreach (var node in nodes)
			{
				node.Nodes = GetBootstrapTreeNodes(int.Parse(node.Tags[0]), archor);
			}
			return nodes;
		}

		/// <summary>
		/// 获取下级子栏目的树节点。
		/// </summary>
		/// <param name="categoryId">栏目标识。</param>
		/// <returns>下级子栏目的树节点。</returns>
		public List<ZTreeNode> GetChildTreeNodes(int? categoryId)
		{
			var query = from category in _dbContext.Categories
						where category.ParentId == categoryId
						orderby category.DisplayOrder descending, category.CreateTime descending
						select new ZTreeNode
						{
							Id = category.Id,
							IsParent = !category.IsLeaf,
							Name = category.Name,
							ContentType = category.ContentType.ToString()
						};

			List<ZTreeNode> nodes = query.ToList();
			foreach (ZTreeNode node in nodes)
			{
				node.Children = GetChildTreeNodes(node.Id);
			}
			return nodes;
		}

		/// <summary>
		/// 获取所有子栏目。
		/// </summary>
		/// <param name="categoryId">栏目标识。</param>
		/// <param name="recursive">是否递归获取。</param>
		/// <returns>所有子栏目。</returns>
		public IList<Category> GetChilds(int categoryId, bool recursive = false)
		{
			var query = from category in _dbContext.Categories
						where category.ParentId == categoryId
						orderby category.DisplayOrder descending, category.CreateTime descending
						select category;
			IList<Category> categories = query.ToList();
			if (!recursive)
			{
				return categories;
			}
			return categories.Concat(categories.SelectMany(category => GetChilds(category.Id, true))).ToList();
		}

		/// <summary>
		/// 获取一级父栏目。
		/// </summary>
		/// <param name="category">当前栏目。</param>
		/// <returns>一级父栏目，如果当前栏目为一级栏目，则直接返回。</returns>
		public Category GetLevel1Category(Category category)
		{
			Category parent = category.Parent;

			while (parent.ParentId != null)
			{
				category = parent;
				parent = category.Parent;
			}
			return category;
		}

		/// <summary>
		/// 获取所有中文的末级文章栏目。
		/// </summary>
		/// <param name="categoryId">顶级栏目的编号。</param>
		/// <returns>所有中文的末级文章栏目。</returns>
		public IList<Category> GetChineseLeafArticleCategories(int categoryId)
		{
			IList<Category> categories = GetChilds(categoryId, true);
			categories = categories.Where(category => !category.IsEnglish && category.IsLeaf && category.ContentType == ContentType.Article)
				.OrderByDescending(category => category.DisplayOrder).ThenByDescending(category => category.CreateTime).ToList();

			return categories;
		}

		/// <summary>
		/// 获取所有递归父栏目。
		/// </summary>
		/// <param name="categoryId">栏目标识。</param>
		/// <param name="includeSelf">是否包含当前栏目。</param>
		/// <returns>所有递归父栏目。</returns>
		public IList<Category> GetRecursiveParents(int categoryId, bool includeSelf)
		{
			IList<Category> categories = new List<Category>();
			Category category = _dbContext.Categories.Find(categoryId);
			if (category != null)
			{
				Category parentCategory = category.Parent;
				while (parentCategory != null)
				{
					categories.Insert(0, parentCategory);
					parentCategory = parentCategory.Parent;
				}

				if (includeSelf)
				{
					categories.Add(category);
				}
			}

			return categories;
		}
	}
}
