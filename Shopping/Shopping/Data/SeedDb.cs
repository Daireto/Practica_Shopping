using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;
using Shopping.Enums;
using Shopping.Helpers;

namespace Shopping.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;

        public SeedDb(DataContext context, IUserHelper userHelper, IBlobHelper blobHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
        }

        public async Task SeedAsync()
        {
            //Database creation and migrations execution
            await _context.Database.EnsureCreatedAsync();

            //Seeds
            await CheckCategoriesAsync();
            await CheckCountriesAsync();
            await CheckRolesAsync();

            //User seeds
            await CheckUsersAsync("1010", 1, "Dairo", "Mosquera", "dairo@yopmail.com", "318 284 6418", "Calle 36 #58-19", "image-dairo.jpg", UserType.Admin);
            await CheckUsersAsync("1020", 18, "Rihanna", "Fenty", "rihanna@yopmail.com", "311 456 6828", "Calle 18 #19-72", "image-riri.jpg", UserType.Admin);
            await CheckUsersAsync("1030", 16, "Lindsey", "Morgan", "lindsey@yopmail.com", "311 456 1885", "Calle 46 #47-27", "image-lindsey.jpg", UserType.Admin);
            await CheckUsersAsync("1040", 51, "Marie", "Avgeropoulos", "marie@yopmail.com", "311 456 9696", "Calle 85 #75-29", "image-marie.jpg", UserType.User);
            await CheckUsersAsync("1050", 21, "Tupac", "Shakur", "tupac@yopmail.com", "311 456 2915", "Calle 26 #16-14", "image-tupac.jpg", UserType.User);
            await CheckUsersAsync("1060", 24, "Curtis", "Jackson", "curtis@yopmail.com", "311 456 7589", "Calle 40 #29-71", "image-curtis.jpg", UserType.User);
            await CheckUsersAsync("1070", 31, "Coco", "Jones", "coco@yopmail.com", "311 456 1124", "Calle 48 #75-45", "image-coco.jpg", UserType.User);
            await CheckUsersAsync("1080", 17, "Megan", "Ruth", "megan@yopmail.com", "311 456 4565", "Calle 16 #12-14", "image-megan.jpg", UserType.User);
            await CheckUsersAsync("1090", 12, "Dua", "Lipa", "dua@yopmail.com", "311 456 4774", "Calle 79 #85-16", "image-lipa.jpg", UserType.User);
            await CheckUsersAsync("2010", 12, "Ramón", "Ayala", "ramon@yopmail.com", "311 456 5695", "Calle 96 #13-13", "image-daddy.jpg", UserType.User);
            await CheckUsersAsync("2020", 52, "Vanessa", "Morgan", "morgan@yopmail.com", "311 456 4645", "Calle 23 #14-18", "image-morgan.jpg", UserType.User);
            await CheckUsersAsync("2030", 47, "Vanessa", "Hudgens", "hudgens@yopmail.com", "311 456 1474", "Calle 65 #14-63", "image-hudgens.jpg", UserType.User);
            await CheckUsersAsync("2040", 36, "Chris", "Tucker", "chris@yopmail.com", "311 456 6323", "Calle 75 #28-96", "image-tucker.jpg", UserType.User);
            await CheckUsersAsync("2050", 22, "Earl", "Simmons", "earl@yopmail.com", "311 456 1121", "Calle 64 #69-68", "image-dmx.jpg", UserType.User);
            await CheckUsersAsync("2060", 41, "Kobe", "Bryant", "kobe@yopmail.com", "311 456 3113", "Calle 26 #13-89", "image-kobe.jpg", UserType.User);
            await CheckUsersAsync("2070", 16, "Beyoncé", "Carter", "beyonce@yopmail.com", "311 456 4010", "Calle 20 #30-45", "image-yonce.jpg", UserType.User);
            await CheckUsersAsync("2080", 26, "Brian", "Henry", "brian@yopmail.com", "311 456 5012", "Calle 58 #92-93", "image-brian.jpg", UserType.User);
            await CheckUsersAsync("2090", 46, "Dwayne", "Johnson", "dwayne@yopmail.com", "311 456 7898", "Calle 94 #38-31", "image-rock.jpg", UserType.User);
            await CheckUsersAsync("3010", 23, "Alicia", "Cook", "alicia@yopmail.com", "311 456 1885", "Calle 54 #42-49", "image-alicia.jpg", UserType.User);
            await CheckUsersAsync("3020", 36, "Normani", "Hamilton", "normani@yopmail.com", "311 456 2623", "Calle 81 #54-21", "image-normani.jpg", UserType.User);
            await CheckUsersAsync("3030", 48, "Ermias", "Asghedom", "ermias@yopmail.com", "311 456 48-39", "Calle 36 #78-50", "image-nipsey.jpg", UserType.User);

            //Product seed
            await CheckProductsAsync();
        }

        private async Task<User> CheckUsersAsync(
            string document,
            int ciudad,
            string firstName,
            string lastName,
            string email,
            string phone,
            string address,
            string imageName,
            UserType userType)
        {
            User user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\images\\users\\{imageName}", "users");
                
                user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = phone,
                    Address = address,
                    Document = document,
                    City = await _context.Cities.FindAsync(ciudad),
                    ImageId = imageId,
                    UserType = userType,
                };
                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());
                string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
            }
            return user;
        }

        private async Task CheckRolesAsync()
        {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());
        }

        private async Task CheckCountriesAsync()
        {
            if (!_context.Countries.Any())
            {
                _context.Countries.Add(new Country
                {
                    Name = "Colombia",
                    States = new List<State>()
                    {
                        new State()
                        {
                            Name = "Antioquia",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Medellín" }, //1
                                new City(){ Name = "Itagüí" }, //2
                                new City(){ Name = "Envigado" }, //3
                                new City(){ Name = "Bello" }, //4
                                new City(){ Name = "Rionegro" } //5
                            }
                        },
                        new State()
                        {
                            Name = "Bogotá",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Usaquen" }, //6
                                new City(){ Name = "Chapinero" }, //7
                                new City(){ Name = "Santa Fé" }, //8
                                new City(){ Name = "Usme" }, //9
                                new City(){ Name = "Bosa" } //10
                            }
                        }
                    }
                });
                _context.Countries.Add(new Country
                {
                    Name = "Estados Unidos",
                    States = new List<State>()
                    {
                        new State()
                        {
                            Name = "Florida",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Orlando" }, //11
                                new City(){ Name = "Miami" }, //12
                                new City(){ Name = "Tampa" }, //13
                                new City(){ Name = "Fort Lauderdale" }, //14
                                new City(){ Name = "Key West" } //15
                            }
                        },
                        new State()
                        {
                            Name = "Texas",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Houston" }, //16
                                new City(){ Name = "San Antonio" }, //17
                                new City(){ Name = "Dallas" }, //18
                                new City(){ Name = "Austin" }, //19
                                new City(){ Name = "El Paso" } //20
                            }
                        },
                        new State()
                        {
                            Name = "Nueva York",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Harlem" }, //21
                                new City(){ Name = "Mount Vernon" }, //22
                                new City(){ Name = "Manhattan" }, //23
                                new City(){ Name = "Queens" }, //24
                                new City(){ Name = "Middletown" } //25
                            }
                        },
                        new State()
                        {
                            Name = "Carolina del Norte",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Fayetteville" }, //26
                                new City(){ Name = "Dunn" }, //27
                                new City(){ Name = "Oxford" }, //28
                                new City(){ Name = "Northwest" }, //29
                                new City(){ Name = "Trinity" } //30
                            }
                        },
                        new State()
                        {
                            Name = "Carolina del Sur",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Columbia" }, //31
                                new City(){ Name = "Clinton" }, //32
                                new City(){ Name = "Laurens" }, //33
                                new City(){ Name = "Lancaster" }, //34
                                new City(){ Name = "Dillon" } //35
                            }
                        },
                        new State()
                        {
                            Name = "Georgia",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Atlanta" }, //36
                                new City(){ Name = "Boston" }, //37
                                new City(){ Name = "Crawford" }, //38
                                new City(){ Name = "Dalton" }, //39
                                new City(){ Name = "Georgetown" } //40
                            }
                        },
                        new State()
                        {
                            Name = "Pensilvania",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Filadelfia" }, //41
                                new City(){ Name = "Arnold" }, //42
                                new City(){ Name = "Greensburg" }, //43
                                new City(){ Name = "Harrisburg" }, //44
                                new City(){ Name = "New Castle" } //45
                            }
                        },
                        new State()
                        {
                            Name = "California",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Hayward" }, //46
                                new City(){ Name = "Salinas" }, //47
                                new City(){ Name = "Los Angeles" }, //48
                                new City(){ Name = "San Diego" }, //49
                                new City(){ Name = "San Jose" } //50
                            }
                        }
                    }
                });
                _context.Countries.Add(new Country
                {
                    Name = "Canadá",
                    States = new List<State>()
                    {
                        new State()
                        {
                            Name = "Ontario",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Thunder Bay" }, //51
                                new City(){ Name = "Ottawa" }, //52
                                new City(){ Name = "Grey" }, //53
                                new City(){ Name = "Toronto" }, //54
                                new City(){ Name = "Hamilton" } //55
                            }
                        },
                        new State()
                        {
                            Name = "Quebec",
                            Cities = new List<City>()
                            {
                                new City(){ Name = "Montreal" }, //56
                                new City(){ Name = "Mauricie" }, //57
                                new City(){ Name = "Laval" }, //58
                                new City(){ Name = "Bas-Saint-Laurent" }, //59
                                new City(){ Name = "Estrie" } //60
                            }
                        }
                    }
                });
            }
            await _context.SaveChangesAsync();
        }

        private async Task CheckCategoriesAsync()
        {
            if (!_context.Categories.Any())
            {
                _context.Categories.Add(new Category { Name = "Tecnología" });
                _context.Categories.Add(new Category { Name = "Ropa" });
                _context.Categories.Add(new Category { Name = "Gamer" });
                _context.Categories.Add(new Category { Name = "Belleza" });
                _context.Categories.Add(new Category { Name = "Nutrición" });
                _context.Categories.Add(new Category { Name = "Calzado" });
                _context.Categories.Add(new Category { Name = "Deportes" });
                _context.Categories.Add(new Category { Name = "Mascotas" });
                _context.Categories.Add(new Category { Name = "Apple" });
            }
            await _context.SaveChangesAsync();
        }

        private async Task CheckProductsAsync()
        {
            if (!_context.Products.Any())
            {
                await AddProductAsync("Adidas Barracuda", 270000M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "adidas_barracuda.png" });
                await AddProductAsync("Adidas Superstar", 250000M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "Adidas_superstar.png" });
                await AddProductAsync("AirPods", 1300000M, 12F, new List<string>() { "Tecnología", "Apple" }, new List<string>() { "airpos.png", "airpos2.png" });
                await AddProductAsync("Audifonos Bose", 870000M, 12F, new List<string>() { "Tecnología" }, new List<string>() { "audifonos_bose.png" });
                await AddProductAsync("Bicicleta Ribble", 12000000M, 6F, new List<string>() { "Deportes" }, new List<string>() { "bicicleta_ribble.png" });
                await AddProductAsync("Camisa Cuadros", 56000M, 24F, new List<string>() { "Ropa" }, new List<string>() { "camisa_cuadros.png" });
                await AddProductAsync("Casco Bicicleta", 820000M, 12F, new List<string>() { "Deportes" }, new List<string>() { "casco_bicicleta.png", "casco.png" });
                await AddProductAsync("iPad", 2300000M, 6F, new List<string>() { "Tecnología", "Apple" }, new List<string>() { "ipad.png" });
                await AddProductAsync("iPhone 13", 5200000M, 6F, new List<string>() { "Tecnología", "Apple" }, new List<string>() { "iphone13.png", "iphone13b.png", "iphone13c.png", "iphone13d.png" });
                await AddProductAsync("Mac Book Pro", 12100000M, 6F, new List<string>() { "Tecnología", "Apple" }, new List<string>() { "mac_book_pro.png" });
                await AddProductAsync("Mancuernas", 370000M, 12F, new List<string>() { "Deportes" }, new List<string>() { "mancuernas.png" });
                await AddProductAsync("Mascarilla Cara", 26000M, 100F, new List<string>() { "Belleza" }, new List<string>() { "mascarilla_cara.png" });
                await AddProductAsync("New Balance 530", 180000M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "newbalance530.png" });
                await AddProductAsync("New Balance 565", 179000M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "newbalance565.png" });
                await AddProductAsync("Nike Air", 233000M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "nike_air.png" });
                await AddProductAsync("Nike Zoom", 249900M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "nike_zoom.png" });
                await AddProductAsync("Buso Adidas Mujer", 134000M, 12F, new List<string>() { "Ropa", "Deportes" }, new List<string>() { "buso_adidas.png" });
                await AddProductAsync("Suplemento Boots Original", 15600M, 12F, new List<string>() { "Nutrición" }, new List<string>() { "Boost_Original.png" });
                await AddProductAsync("Whey Protein", 252000M, 12F, new List<string>() { "Nutrición" }, new List<string>() { "whey_protein.png" });
                await AddProductAsync("Arnes Mascota", 25000M, 12F, new List<string>() { "Mascotas" }, new List<string>() { "arnes_mascota.png" });
                await AddProductAsync("Cama Mascota", 99000M, 12F, new List<string>() { "Mascotas" }, new List<string>() { "cama_mascota.png" });
                await AddProductAsync("Teclado Gamer", 67000M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "teclado_gamer.png" });
                await AddProductAsync("Silla Gamer", 980000M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "silla_gamer.png" });
                await AddProductAsync("Mouse Gamer", 132000M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "mouse_gamer.png" });
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddProductAsync(string name, decimal price, float stock, List<string> categories, List<string> images)
        {
            Product product = new()
            {
                Description = name,
                Name = name,
                Price = price,
                Stock = stock,
                ProductCategories = new List<ProductCategory>(),
                ProductImages = new List<ProductImage>()
            };

            foreach (string category in categories)
            {
                product.ProductCategories.Add(new ProductCategory { Category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == category) });
            }

            foreach (string image in images)
            {
                Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\images\\products\\{image}", "products");
                product.ProductImages.Add(new ProductImage { ImageId = imageId });
            }

            _context.Products.Add(product);
        }

    }
}
