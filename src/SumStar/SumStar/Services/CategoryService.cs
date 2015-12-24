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
		/// <returns>下级子栏目的树节点。</returns>
		public List<ZTreeNode> GetChildTreeNodes(int? categoryId)
		{
			var query = from category in _dbContext.Categories
						where category.ParentId == categoryId
						orderby category.DisplayOrder
						select new ZTreeNode
						{
							Id = category.Id,
							Name = category.Name,
							IsParent = true,
							ContentType = category.ContentType.ToString()
						};
			return query.ToList();
		}

		/// <summary>
		/// 获取所有递归子栏目。
		/// </summary>
		/// <param name="categoryId">栏目标识。</param>
		/// <returns>所有递归子栏目。</returns>
		public IEnumerable<Category> GetRecursiveChilds(int categoryId)
		{
			var query = from category in _dbContext.Categories
						where category.ParentId == categoryId
						orderby category.DisplayOrder
						select category;
			IList<Category> categories = query.ToList();
			return categories.Concat(categories.SelectMany(category => GetRecursiveChilds(category.Id)));
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
