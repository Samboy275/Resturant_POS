using POS.Database;
using POS.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace POS.Controllers
{
    public class MenuController
    {
        private readonly PosDbContext _context;

        public MenuController()
        {
            _context = new PosDbContext();
        }

        // Categories
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .Include(c => c.MenuItems)
                .ToListAsync();
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        // Menu Items
        public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
        {
            return await _context.MenuItems
                .Where(m => m.CategoryId == categoryId && m.IsActive)
                .ToListAsync();
        }

        public async Task<MenuItem> GetMenuItemAsync(int id)
        {
            return await _context.MenuItems.FindAsync(id);
        }

        public async Task<bool> AddMenuItemAsync(MenuItem menuItem)
        {
            _context.MenuItems.Add(menuItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateMenuItemAsync(MenuItem menuItem)
        {
            _context.MenuItems.Update(menuItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item != null)
            {
                item.IsActive = false;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
