-- E-Commerce Schema

CREATE TABLE IF NOT EXISTS categories (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    slug TEXT NOT NULL UNIQUE,
    parent_id INTEGER REFERENCES categories(id) ON DELETE SET NULL,
    description TEXT,
    icon TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS products (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    slug TEXT NOT NULL UNIQUE,
    description TEXT,
    price REAL NOT NULL,
    stock INTEGER DEFAULT 0,
    category_id INTEGER REFERENCES categories(id) ON DELETE SET NULL,
    image TEXT,
    active INTEGER DEFAULT 1,
    featured INTEGER DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    role TEXT DEFAULT 'customer',
    address TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS orders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER REFERENCES users(id),
    status TEXT DEFAULT 'pending',
    total REAL NOT NULL,
    shipping_address TEXT,
    notes TEXT,
    delivery_method TEXT,
    delivery_cost REAL DEFAULT 0,
    customer_email TEXT,
    customer_name TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS delivery_options (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    price REAL NOT NULL,
    active INTEGER DEFAULT 1,
    min_order_total REAL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS order_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER REFERENCES orders(id) ON DELETE CASCADE,
    product_id INTEGER REFERENCES products(id),
    quantity INTEGER NOT NULL,
    unit_price REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS rate_limits (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    action TEXT NOT NULL,
    ip_address TEXT NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_categories_parent ON categories(parent_id);
CREATE INDEX IF NOT EXISTS idx_products_category_active ON products(category_id, active);
CREATE INDEX IF NOT EXISTS idx_orders_user_created ON orders(user_id, created_at);
CREATE INDEX IF NOT EXISTS idx_orders_status_created ON orders(status, created_at);
CREATE INDEX IF NOT EXISTS idx_order_items_order ON order_items(order_id);
CREATE INDEX IF NOT EXISTS idx_order_items_product ON order_items(product_id);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
CREATE INDEX IF NOT EXISTS idx_rate_limits_lookup ON rate_limits(action, ip_address, created_at);

INSERT OR IGNORE INTO categories (id, name, slug, parent_id, description, icon) VALUES
(1, 'Electronics', 'electronics', NULL, 'Gadgets, devices and tech', '💻'),
(2, 'Clothing', 'clothing', NULL, 'Apparel for all occasions', '👕'),
(3, 'Home & Garden', 'home-garden', NULL, 'For the home and garden', '🏠'),
(4, 'Laptops', 'laptops', 1, 'Portable computers', '💻'),
(5, 'Phones', 'phones', 1, 'Smartphones and accessories', '📱'),
(6, 'Audio', 'audio', 1, 'Headphones, speakers and more', '🎧'),
(7, 'Mens', 'mens', 2, 'Menswear', '👔'),
(8, 'Womens', 'womens', 2, 'Womenswear', '👗'),
(9, 'Kitchen', 'kitchen', 3, 'Kitchen appliances and tools', '🍳'),
(10, 'Garden Tools', 'garden-tools', 3, 'Tools for the garden', '🌱'),
(14, 'Gaming', 'gaming', 1, 'Gaming products.', '🎮'),
(15, 'Toys', 'toys', NULL, 'Various toy categories.', '🧸'),
(16, 'Discovery Toys', 'discovery-toys', 15, 'Learning & discovery toys.', '🧸'),
(17, 'LEGO', 'lego', 16, 'All LEGO products.', '🧸'),
(18, 'Pet', 'pet', NULL, 'Pet products.', '🐈'),
(20, 'Cat', 'cat', 18, 'Cat products.', '🐈‍⬛'),
(21, 'Dog', 'dog', 18, 'Dog products.', '🦮'),
(22, 'Other Animals', 'other-animals', 18, 'Products for other pets.', '🐻');

INSERT OR IGNORE INTO products (id, name, slug, description, price, stock, category_id, image, active, featured, created_at) VALUES
(1, 'ProBook Laptop 15"', 'probook-laptop-15', 'A powerful 15-inch laptop with 16GB RAM, 512GB SSD and a stunning display. Perfect for work and creativity.', 899.99, 11, 4, 'img_69bd3779ba9bf9.94612358.jpg', 1, 0, '2026-03-20 09:58:43'),
(2, 'UltraPhone X12', 'ultraphone-x12', 'The latest flagship smartphone featuring a triple-camera system, 5G connectivity and all-day battery life.', 749, 22, 5, 'img_69bd376dc40e67.91131762.jpg', 1, 0, '2026-03-20 09:58:43'),
(3, 'Studio Wireless Headphones', 'studio-wireless-headphones', 'Premium over-ear headphones with active noise cancellation and 30-hour battery. Studio-quality sound anywhere.', 299.95, 42, 6, 'img_69bd3762e90036.79611024.jpg', 1, 0, '2026-03-20 09:58:43'),
(4, 'Classic Oxford Shirt', 'classic-oxford-shirt', 'Timeless Oxford weave cotton shirt. Versatile enough for the office or weekend. Available in multiple colours.', 59.99, 79, 7, 'img_69bd3757beda69.95266902.jpg', 1, 0, '2026-03-20 09:58:43'),
(5, 'Merino Wool Jumper', 'merino-wool-jumper', 'Soft, lightweight and warm. This merino wool jumper is a wardrobe essential for the cooler months.', 89, 34, 8, 'img_69bd3728f36b86.03617973.jpg', 1, 0, '2026-03-20 09:58:43'),
(6, 'Espresso Machine Pro', 'espresso-machine-pro', 'Barista-grade espresso at home. 15-bar pump pressure, built-in grinder, and milk frother included.', 449, 12, 9, 'img_69bd371c0a7f91.86115081.jpg', 1, 0, '2026-03-20 09:58:43'),
(7, 'Carbon Steel Garden Trowel', 'carbon-steel-garden-trowel', 'Professional-grade carbon steel trowel with an ergonomic hardwood handle. Built to last a lifetime.', 24.99, 59, 10, 'img_69bd370cad5f19.97444002.jpg', 1, 0, '2026-03-20 09:58:43'),
(8, 'MiniBook Air 13"', 'minibook-air-13', 'Featherlight 13-inch ultrabook. All-day battery, fanless design, and a gorgeous Retina-class display.', 1099, 7, 4, 'img_69bd36fea24797.63486949.jpg', 1, 1, '2026-03-20 09:58:43'),
(9, 'Bluetooth Speaker Cube', 'bluetooth-speaker-cube', 'Compact, waterproof Bluetooth speaker delivering surprisingly big sound. Perfect for outdoors.', 79.95, 55, 6, 'img_69bd3678dda058.99673075.jpg', 1, 0, '2026-03-20 09:58:43'),
(10, 'Cast Iron Skillet 28cm', 'cast-iron-skillet-28cm', 'Pre-seasoned cast iron skillet. Sears, bakes, grills and fries. Virtually indestructible.', 39.99, 28, 9, 'img_69bd39df6196a1.38026816.jpg', 1, 0, '2026-03-20 09:58:43'),
(11, 'iPhone 17 Pro Max', 'iphone-17-pro-max', 'iPhone 17 Pro Max. The most powerful iPhone ever.', 1199, 2, 5, 'img_69bd46751926a3.56450672.webp', 1, 0, '2026-03-20 10:36:01'),
(12, 'Playstation 5', 'playstation-5', 'PlayStation 5 Console. The PS5 console unleashes new gaming possibilities that you never anticipated.', 479.99, 0, 14, 'img_69bd4d700341b0.13942450.webp', 1, 0, '2026-03-20 10:36:43'),
(13, 'Nintendo Switch 2 Console', 'nintendo-switch-2-console', 'The next evolution of the Nintendo Switch console is here!', 385.99, 28, 14, 'img_69bd4ec03e4005.93093090.webp', 1, 1, '2026-03-20 10:37:35'),
(14, 'LEGO Speed Champions Mercedes-AMG F1 W15 Race Car Toy', 'lego-speed-champions-mercedes-amg-f1-w15-race-car-toy', 'F1 fans and kids aged 10 and up can enjoy exciting race action with this LEGO Speed Champions toy.', 23, 11, 17, 'img_69bd50dc58de60.49794891.webp', 1, 0, '2026-03-20 13:51:24'),
(15, 'LEGO Star Wars BB-8 Astromech Droid', 'lego-star-wars-bb-8-astromech-droid', 'STAR WARS LEGO DROID FIGURE capturing the charm of BB-8 from The Force Awakens.', 80, 9, 17, 'img_69bd54828d6ee5.27048989.webp', 1, 0, '2026-03-20 14:06:58'),
(16, 'Science Mad 20cm Illuminated Night Globe', 'science-mad-20cm-illuminated-night-globe', 'A miniature model of our planet at your finger-tips, giving you access to continents, oceans, countries and major cities.', 22, 0, 16, 'img_69bd5739b3ac33.55848488.webp', 1, 0, '2026-03-20 14:18:33'),
(17, 'Pokémon Mega Charizard X Ex Trading Card', 'pok-mon-mega-charizard-x-ex-trading-card', 'Under the right conditions Charizard can Mega Evolve! Choose the raging blue flames of Mega Charizard X ex.', 22, 0, 15, 'img_69bd57c1b0b871.21861055.webp', 1, 0, '2026-03-20 14:20:49'),
(18, 'SIM Free Samsung Galaxy S26 Ultra 5G 512GB AI Phone Violet', 'sim-free-samsung-galaxy-s26-ultra-5g-512gb-ai-phone-violet', 'Meet Galaxy S26 Ultra. Featuring our most advanced display yet and powerful Galaxy AI.', 1449, 1, 5, 'img_69bd589eb18a70.09464393.webp', 1, 0, '2026-03-20 14:24:30'),
(19, 'Petface House Scratcher', 'petface-house-scratcher', 'This Cat house scratcher gives your cat a place to have fun, exercise, explore, scratch and just relax.', 70, 14, 20, 'img_69bd632bc82e28.46608911.webp', 1, 0, '2026-03-20 15:09:31'),
(20, 'Cat Lounge and Play Scratcher', 'cat-lounge-and-play-scratcher', 'A cozy home for cats featuring a roomy condo, relaxing lounge basket, and two perfectly placed perches.', 88, 6, 20, 'img_69bd63cfc64980.14864876.webp', 1, 0, '2026-03-20 15:12:15'),
(21, 'Ninja 7.6L Foodi Dual Zone Air Fryer and Dehydrator', 'ninja-7-6l-foodi-dual-zone-air-fryer-and-dehydrator', 'The air fryer that cooks 2 foods, 2 ways, and finishes at the same time. Extra-large 7.6L capacity.', 200, 22, 9, 'img_69cfa1d4e5f6c3.95570107.jpg', 1, 0, '2026-04-03 11:17:40'),
(22, 'McGregor 23cm Cordless Grass Trimmer - 18V', 'mcgregor-23cm-cordless-grass-trimmer-18v', 'This McGregor 18V 23cm grass trimmer is ideal for cutting fine to coarse grass - anywhere.', 36, 14, 10, 'img_69d25436496576.37754497.jpg', 1, 0, '2026-04-05 12:23:18');

INSERT OR IGNORE INTO delivery_options (id, name, price, active, min_order_total) VALUES
(1, 'Standard Delivery', 3.99, 1, 0),
(2, 'Next Day Delivery', 6.99, 1, 0),
(3, 'Free Shipping (Over £50)', 0.00, 1, 50.00);
