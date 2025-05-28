using POS.Database; // Correct using directive for DbContext
using POS.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace POS.Controllers
{
    public class MenuController
    {
        private readonly POSDbContext _context; // Renamed to POSDbContext for consistency

        // CRITICAL: Change this constructor to accept POSDbContext via DI
        public MenuController(POSDbContext context) // This is the corrected constructor
        {
            _context = context;
        }

        // Removed the problematic public MenuController() { _context = new PosDbContext(); }


        // Categories
        public async Task<List<Category>> GetCategoriesAsync()
        {
            // If POSDbContext has a global query filter for IsActive on Category,
            // the .Where(c => c.IsActive) might be redundant but doesn't hurt.
            // Including MenuItems for each category can be useful for display or admin.
            return await _context.Categories
                .Where(c => c.IsActive)
                .Include(c => c.MenuItems.Where(m => m.IsActive)) // Only include active menu items within categories
                .ToListAsync();
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            // Good to ensure IsActive is set if it's not handled by default in the model/DB
            category.IsActive = true;
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category); // Marks category as modified
            return await _context.SaveChangesAsync() > 0;
        }

        // Menu Items
        public async Task<List<MenuItem>> GetMenuItemsByCategoryIdAsync(int categoryId)
        {
            // Good to filter by IsActive for menu display
            return await _context.MenuItems
                .Where(m => m.CategoryId == categoryId && m.IsActive)
                .ToListAsync();
        }

        public async Task<MenuItem> GetMenuItemAsync(int id)
        {
            // FindAsync is efficient for primary key lookup, but doesn't respect global filters
            // or IsActive in this case. Consider FirstOrDefaultAsync for consistency with IsActive.
            var item = await _context.MenuItems.FindAsync(id);
            if (item != null && item.IsActive) return item;
            return null; // Ensure only active items are returned
            // Alternative: return await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
        }

        public async Task<bool> AddMenuItemAsync(MenuItem menuItem)
        {
            // Good to ensure IsActive is set
            menuItem.IsActive = true;
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
                item.IsActive = false; // Soft delete - EXCELLENT PRACTICE
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}