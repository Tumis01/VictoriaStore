using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.Models;

namespace VictoriaStore.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // Ensure database is created and migrations applied
            await context.Database.MigrateAsync();

            // ── 1. Seed Roles ──────────────────────────────────────
            string[] roles = { "SuperAdmin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
                    logger.LogInformation("Created role: {Role}", role);
                }
            }

            // ── 2. Seed Admin User ─────────────────────────────────
            const string adminEmail = "olumadetunmise@gmail.com";
            const string adminPassword = "Password123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FullName = "Victoria James",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                    logger.LogInformation("Created admin user: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Ensure existing user has SuperAdmin role
                if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                    logger.LogInformation("Assigned SuperAdmin role to existing user: {Email}", adminEmail);
                }
            }

            // ── 3. Seed Categories ─────────────────────────────────
            if (!await context.Categories.AnyAsync())
            {
                var categories = GetSeedCategories();
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} categories", categories.Count);

                // ── 4. Seed Products (after categories are saved) ──
                var products = GetSeedProducts(categories);
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} products", products.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  SEED CATEGORIES — 5 categories
    // ═══════════════════════════════════════════════════════════════
    private static List<Category> GetSeedCategories()
    {
        return new List<Category>
        {
            new()
            {
                Name = "Household Items",
                Slug = "household-items",
                Description = "Essential products for every Nigerian home — from storage solutions to cleaning supplies and home décor.",
                DisplayOrder = 1,
                IsActive = true
            },
            new()
            {
                Name = "Kitchen Utensils",
                Slug = "kitchen-utensils",
                Description = "Premium cookware, non-stick pans, knives, and accessories to upgrade your kitchen experience.",
                DisplayOrder = 2,
                IsActive = true
            },
            new()
            {
                Name = "Fashion & Giftpacks",
                Slug = "fashion-giftpacks",
                Description = "Trendy fashion accessories, gift boxes, and curated packs perfect for every occasion.",
                DisplayOrder = 3,
                IsActive = true
            },
            new()
            {
                Name = "Accessories",
                Slug = "accessories",
                Description = "Watches, jewelry, bags, and everyday accessories to complete your look.",
                DisplayOrder = 4,
                IsActive = true
            },
            new()
            {
                Name = "Beauty & Personal Care",
                Slug = "beauty-personal-care",
                Description = "Skincare, hair care, fragrances, and grooming essentials for men and women.",
                DisplayOrder = 5,
                IsActive = true
            }
        };
    }

    // ═══════════════════════════════════════════════════════════════
    //  SEED PRODUCTS — 6 products per category (30 total)
    // ═══════════════════════════════════════════════════════════════
    private static List<Product> GetSeedProducts(List<Category> categories)
    {
        var household = categories.First(c => c.Slug == "household-items");
        var kitchen = categories.First(c => c.Slug == "kitchen-utensils");
        var fashion = categories.First(c => c.Slug == "fashion-giftpacks");
        var accessories = categories.First(c => c.Slug == "accessories");
        var beauty = categories.First(c => c.Slug == "beauty-personal-care");

        var products = new List<Product>();

        // ── Household Items (6) ────────────────────────────────
        products.AddRange(new[]
        {
            CreateProduct("Stackable Storage Container Set", "stackable-storage-containers",
                "Set of 5 BPA-free stackable storage containers with airtight lids. Perfect for organizing your pantry, wardrobe, or bathroom.",
                8500, null, "VJS-HH-001", 45, household.Id,
                new[] { "Clear", "Blue", "Pink" }),
            CreateProduct("Premium Mop & Bucket System", "premium-mop-bucket",
                "360° spin mop with stainless steel bucket and microfiber heads. Makes floor cleaning effortless.",
                12000, 9500, "VJS-HH-002", 30, household.Id,
                new[] { "Grey", "Blue" }),
            CreateProduct("Decorative LED Fairy Lights", "decorative-led-fairy-lights",
                "10-meter warm white LED string lights with 8 lighting modes. USB-powered, perfect for bedrooms and events.",
                4500, 3200, "VJS-HH-003", 80, household.Id,
                new[] { "Warm White", "Multi-Color" }),
            CreateProduct("Foldable Laundry Basket – Large", "foldable-laundry-basket-large",
                "Collapsible bamboo-frame laundry hamper with handles. Saves space when not in use. 65L capacity.",
                6800, null, "VJS-HH-004", 25, household.Id,
                new[] { "Grey", "Beige" }),
            CreateProduct("Wall-Mounted Shelf Organizer", "wall-mounted-shelf-organizer",
                "Rustic wooden floating shelf set (3 pieces). Easy to install, great for books, plants, and décor.",
                9200, 7500, "VJS-HH-005", 18, household.Id,
                new[] { "Brown", "White" }),
            CreateProduct("Automatic Soap Dispenser", "automatic-soap-dispenser",
                "Touchless infrared soap dispenser with 350ml capacity. Hygienic, battery-operated, waterproof IPX4.",
                5500, null, "VJS-HH-006", 40, household.Id,
                new[] { "White", "Silver" }),
        });

        // ── Kitchen Utensils (6) ───────────────────────────────
        products.AddRange(new[]
        {
            CreateProduct("Non-Stick Frying Pan Set", "non-stick-frying-pan-set",
                "3-piece granite-coated non-stick frying pan set (20cm, 24cm, 28cm). PFOA-free, works on all cooktops including induction.",
                15000, 11500, "VJS-KT-001", 35, kitchen.Id,
                new[] { "Black", "Red" }),
            CreateProduct("Stainless Steel Knife Block Set", "stainless-steel-knife-set",
                "7-piece professional knife set with wooden block. Includes chef's knife, bread knife, utility knife, paring knife, and scissors.",
                18500, null, "VJS-KT-002", 20, kitchen.Id,
                new[] { "Silver/Wood" }),
            CreateProduct("Electric Blender – 1.5L", "electric-blender-1500ml",
                "Powerful 800W blender with 1.5L glass jar, 3-speed settings plus pulse. Perfect for smoothies, soups, and pepper grinding.",
                22000, 18000, "VJS-KT-003", 15, kitchen.Id,
                new[] { "Black", "White" }),
            CreateProduct("Silicone Cooking Utensil Set", "silicone-cooking-utensil-set",
                "12-piece heat-resistant silicone utensil set with wooden handles. Includes spatula, ladle, tongs, whisk, and storage holder.",
                9800, null, "VJS-KT-004", 50, kitchen.Id,
                new[] { "Teal", "Red", "Black" }),
            CreateProduct("Ceramic Dinner Plate Set", "ceramic-dinner-plate-set",
                "16-piece ceramic dinnerware set for 4. Includes dinner plates, side plates, bowls, and mugs. Dishwasher and microwave safe.",
                25000, 20000, "VJS-KT-005", 12, kitchen.Id,
                new[] { "White/Gold", "Blue/White" }),
            CreateProduct("Spice Rack Organizer – 20 Jars", "spice-rack-organizer-20",
                "Rotating bamboo spice rack with 20 glass jars and labels. Keeps your spices organized and within reach.",
                7500, null, "VJS-KT-006", 28, kitchen.Id,
                new[] { "Natural Bamboo" }),
        });

        // ── Fashion & Giftpacks (6) ────────────────────────────
        products.AddRange(new[]
        {
            CreateProduct("Luxury Gift Box – For Her", "luxury-gift-box-for-her",
                "Curated gift box with scented candle, silk scrunchie set, personalized notebook, and bath bomb collection. Beautifully packaged.",
                16000, 13500, "VJS-FG-001", 22, fashion.Id,
                new[] { "Rose Gold", "Lavender" }),
            CreateProduct("Men's Grooming Gift Set", "mens-grooming-gift-set",
                "Premium gift set with beard oil, beard balm, wooden comb, shaving brush, and canvas travel pouch.",
                14500, null, "VJS-FG-002", 18, fashion.Id,
                new[] { "Brown/Black" }),
            CreateProduct("Ankara Print Tote Bag", "ankara-print-tote-bag",
                "Handmade African print tote bag with zip closure and inner pocket. Durable cotton canvas with leather straps.",
                8500, 6800, "VJS-FG-003", 40, fashion.Id,
                new[] { "Multi-Print A", "Multi-Print B", "Multi-Print C" }),
            CreateProduct("Couple's Matching Watch Set", "couples-matching-watch-set",
                "His and hers minimalist quartz watches with genuine leather straps. Japanese movement, water-resistant.",
                28000, 22000, "VJS-FG-004", 10, fashion.Id,
                new[] { "Black/Rose Gold", "Brown/Silver" }),
            CreateProduct("Scented Candle Gift Collection", "scented-candle-collection",
                "Set of 4 hand-poured soy wax candles in elegant glass jars. Scents: Vanilla, Lavender, Jasmine, Cinnamon. 40-hour burn time each.",
                12000, null, "VJS-FG-005", 30, fashion.Id,
                new[] { "Assorted" }),
            CreateProduct("Birthday Surprise Box – Unisex", "birthday-surprise-box",
                "All-in-one birthday gift box with mug, keychain, chocolate box, greeting card, and confetti popper. Perfect for any age.",
                9500, 7800, "VJS-FG-006", 35, fashion.Id,
                new[] { "Pink Theme", "Blue Theme", "Gold Theme" }),
        });

        // ── Accessories (6) ────────────────────────────────────
        products.AddRange(new[]
        {
            CreateProduct("Minimalist Leather Wallet", "minimalist-leather-wallet",
                "Slim genuine leather bifold wallet with RFID blocking. 6 card slots, 2 cash compartments, and ID window.",
                7500, null, "VJS-AC-001", 55, accessories.Id,
                new[] { "Black", "Brown", "Tan" }),
            CreateProduct("Wireless Bluetooth Earbuds", "wireless-bluetooth-earbuds",
                "TWS earbuds with active noise cancellation, 30-hour battery life, IPX5 waterproof. Touch controls, USB-C charging.",
                18000, 14500, "VJS-AC-002", 25, accessories.Id,
                new[] { "Black", "White" }),
            CreateProduct("Stainless Steel Water Bottle – 750ml", "stainless-steel-water-bottle",
                "Double-wall vacuum insulated bottle. Keeps drinks cold 24hrs or hot 12hrs. BPA-free, leak-proof lid.",
                5500, null, "VJS-AC-003", 60, accessories.Id,
                new[] { "Midnight Blue", "Rose Gold", "Matte Black" }),
            CreateProduct("Unisex Baseball Cap – Premium", "unisex-baseball-cap-premium",
                "Adjustable cotton twill cap with embroidered logo. One size fits all. UV protection, breathable mesh back.",
                4500, 3500, "VJS-AC-004", 70, accessories.Id,
                new[] { "Black", "Navy", "Olive", "Beige" }),
            CreateProduct("Crossbody Sling Bag", "crossbody-sling-bag",
                "Waterproof nylon sling bag with anti-theft zip. Multiple compartments, adjustable strap. Perfect for daily commute.",
                9800, null, "VJS-AC-005", 32, accessories.Id,
                new[] { "Black", "Grey", "Army Green" }),
            CreateProduct("Pearl & Gold Jewelry Set", "pearl-gold-jewelry-set",
                "Elegant faux pearl necklace, bracelet, and earring set with 18k gold plating. Comes in a velvet gift box.",
                13500, 10800, "VJS-AC-006", 15, accessories.Id,
                new[] { "Gold/White", "Silver/White" }),
        });

        // ── Beauty & Personal Care (6) ─────────────────────────
        products.AddRange(new[]
        {
            CreateProduct("Vitamin C Face Serum – 30ml", "vitamin-c-face-serum",
                "20% Vitamin C + Hyaluronic Acid + Vitamin E serum. Brightens skin, reduces dark spots, boosts collagen. For all skin types.",
                8500, 6500, "VJS-BC-001", 40, beauty.Id,
                new[] { "30ml" }),
            CreateProduct("Natural Hair Growth Oil", "natural-hair-growth-oil",
                "100% natural blend of castor oil, coconut oil, rosemary, and peppermint. Stimulates growth, reduces breakage. 100ml bottle.",
                5500, null, "VJS-BC-002", 55, beauty.Id,
                new[] { "100ml" }),
            CreateProduct("Electric Facial Cleansing Brush", "electric-facial-brush",
                "Rechargeable sonic facial brush with 3 brush heads and 5 speed settings. Waterproof IPX7. Deep pore cleansing.",
                12000, 9500, "VJS-BC-003", 20, beauty.Id,
                new[] { "Pink", "Mint Green", "White" }),
            CreateProduct("Men's Eau de Parfum – 100ml", "mens-eau-de-parfum",
                "Long-lasting woody-spicy fragrance with notes of bergamot, cedar, and amber. 8+ hour longevity.",
                15000, null, "VJS-BC-004", 18, beauty.Id,
                new[] { "100ml" }),
            CreateProduct("Shea Butter Body Cream – 500ml", "shea-butter-body-cream",
                "Rich, unrefined shea butter cream with cocoa butter and vitamin E. Deep moisturizing, non-greasy. Made in Nigeria.",
                4200, 3500, "VJS-BC-005", 65, beauty.Id,
                new[] { "Original", "Lavender", "Coconut" }),
            CreateProduct("Complete Skincare Routine Kit", "complete-skincare-kit",
                "5-step skincare set: cleanser, toner, serum, moisturizer, and SPF 50 sunscreen. 30-day supply for combination skin.",
                22000, 18000, "VJS-BC-006", 12, beauty.Id,
                new[] { "Standard Kit" }),
        });

        return products;
    }

    private static Product CreateProduct(string name, string slug, string description,
        decimal price, decimal? salePrice, string sku, int stock, Guid categoryId, string[] colors)
    {
        return new Product
        {
            Name = name,
            Slug = slug,
            Description = description,
            Price = price,
            SalePrice = salePrice,
            SKU = sku,
            StockQuantity = stock,
            CategoryId = categoryId,
            Colors = colors.ToList(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
            UpdatedAt = DateTime.UtcNow
        };
    }
}
