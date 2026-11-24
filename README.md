# LoveCat · โปรเจกต์วิชา CSI402

LoveCat เป็นแพลตฟอร์มขายสินค้าสัตว์เลี้ยงหลายร้าน ลูกค้าซื้อจากหลายร้านและชำระครั้งเดียว ร้านผู้ขายจัดการสินค้า สต็อก และออเดอร์ของตัวเอง ฝ่ายบัญชีตรวจสลิปตามยอดชำระรวมและบันทึกโอนแยกตามออเดอร์ ส่วน LoveCat คิดค่าคอมมิชชัน 15% จากราคาที่ร้านตั้ง

ระบบใช้ ASP.NET Core MVC บน .NET 10, C#, Razor, Entity Framework Core และ MySQL ฐาน `csi402db`

## เปิดไฟล์ไหนเมื่ออาจารย์ถาม

ใน VS Code กด **Ctrl+P** แล้วพิมพ์ชื่อไฟล์ จากนั้นกด **Ctrl+F** ค้นคำในคอลัมน์ขวา ถ้ามีชื่อไฟล์ซ้ำ ให้พิมพ์โฟลเดอร์ด้วย เช่น `Account/Product.cshtml`

| อาจารย์ถามว่า | เปิดไฟล์ก่อน | ค้นคำนี้ / ตามไปไฟล์ไหน |
|---|---|---|
| หน้า `/Account/Product` อยู่ไหน | [Account/Product.cshtml](WebApplication1/Views/Account/Product.cshtml) | `เพิ่มสินค้า` → ฟอร์มส่งไป [ShopController](WebApplication1/Controllers/ShopController.cs) ค้น `SaveProduct` |
| หน้า `/Home/Product` กับหน้าร้านต่างกันตรงไหน | [HomeController](WebApplication1/Controllers/HomeController.cs) | `BuildCatalogAsync` → เปิด [Product.cshtml](WebApplication1/Views/Home/Product.cshtml) หรือ [Shop.cshtml](WebApplication1/Views/Home/Shop.cshtml) ตาม URL แล้วตามไป [_StorefrontCatalog](WebApplication1/Views/Shared/_StorefrontCatalog.cshtml) |
| ปุ่มบวกเพิ่มลงตะกร้าอยู่ไหน | [_ProductCard.cshtml](WebApplication1/Views/Shared/_ProductCard.cshtml) | `data-add-to-cart` → [main.js](WebApplication1/wwwroot/js/app/main.js) ค้น `cart.add(` → [cart.js](WebApplication1/wwwroot/js/app/cart.js) เก็บรายการในเบราว์เซอร์ |
| ปุ่มบวกจำนวนในตะกร้าอยู่ไหน | [drawers.js](WebApplication1/wwwroot/js/app/drawers.js) | `data-qty-step` → `cart.setQty` → [_ShopDrawers.cshtml](WebApplication1/Views/Shared/_ShopDrawers.cshtml) เป็นโครงหน้าตะกร้า |
| ระบบคิดยอดใหม่หรือเขียนออเดอร์ตอนไหน | [CheckoutController](WebApplication1/Controllers/CheckoutController.cs) | `Quote` ดูยอดที่คำนวณ ส่วน `PlaceOrder` เรียก [CheckoutService](WebApplication1/Services/Orders/CheckoutService.cs) ให้บันทึกออเดอร์ |
| จ่ายด้วยสลิปเดียวแล้วแยกตามร้านตรงไหน | [CheckoutService](WebApplication1/Services/Orders/CheckoutService.cs) | `GroupBy(line => line.ShopId)` |
| ฝ่ายบัญชีขอสลิปใหม่หรือยกเลิกตรงไหน | [OrderStatus.cshtml](WebApplication1/Views/Account/OrderStatus.cshtml) | `ขอสลิปใหม่` → [PaymentsController](WebApplication1/Controllers/PaymentsController.cs) ค้น `Review` หรือ `Cancel` |
| ใส่เลขพัสดุหรือยืนยันได้รับสินค้าตรงไหน | [_ShipmentForm.cshtml](WebApplication1/Views/Account/Sections/_ShipmentForm.cshtml) และ [_CustomerOrders.cshtml](WebApplication1/Views/Account/Sections/_CustomerOrders.cshtml) | `trackingNumber` และ `ยืนยันได้รับสินค้า` |
| กรองออเดอร์ยังไม่โอนตรงไหน | [OrderStatus.cshtml](WebApplication1/Views/Account/OrderStatus.cshtml) | `name="payout"` → [PayoutQueries](WebApplication1/Data/PayoutQueries.cs) ค้น `PayoutFilter.Unpaid` |
| หน้าขึ้น 403 อยู่ไฟล์ไหน | [Forbidden.cshtml](WebApplication1/Views/Shared/Forbidden.cshtml) | `หน้านี้ไม่ได้เปิดให้บัญชีของคุณนะ` → [RequireRoleAttribute](WebApplication1/Filters/RequireRoleAttribute.cs) |

### หา View จาก URL

| URL | Controller และ View |
|---|---|
| `/` | `HomeController.Index` → [Views/Home/Index.cshtml](WebApplication1/Views/Home/Index.cshtml) |
| `/Home/<Action>` | [HomeController](WebApplication1/Controllers/HomeController.cs) → `Views/Home/<Action>.cshtml` เช่น `Product`, `ProductDetail`, `Shop`, `Promotion`, `Help`, `Login`, `Register` และ `ForgotPassword` |
| `/Account/<Action>` | [AccountController](WebApplication1/Controllers/AccountController.cs) → `Views/Account/<Action>.cshtml` เช่น `Index`, `Product`, `Packing`, `Shipped`, `Jobs`, `MyDeliveries`, `Orders`, `Tracking`, `OrderStatus`, `Ledger`, `Finance` และ `Profile` |
| `/Shop/Messages`, `/Shop/Reviews` | [ShopController](WebApplication1/Controllers/ShopController.cs) → [Views/Shop](WebApplication1/Views/Shop/) |
| `/ShopContact/Conversation` | [ShopContactController](WebApplication1/Controllers/ShopContactController.cs) → [Conversation.cshtml](WebApplication1/Views/ShopContact/Conversation.cshtml) |
| `/Orders/Label` | [OrdersController](WebApplication1/Controllers/OrdersController.cs) → [Label.cshtml](WebApplication1/Views/Orders/Label.cshtml) |
| `/Admin/Adduser`, `/Admin/userlist` | [AdminController](WebApplication1/Controllers/AdminController.cs) → [Views/Admin](WebApplication1/Views/Admin/) |
| `/Error/Status/404` | [ErrorController](WebApplication1/Controllers/ErrorController.cs) → [Status.cshtml](WebApplication1/Views/Error/Status.cshtml) |

URL ที่เป็นคำสั่งหลังคลิก เช่น `/Coupons/Save` ให้หา Action ใน Controller ตามชื่อ URL ส่วน `/Checkout/Quote` คืนข้อมูล JSON ให้ JavaScript ชื่อ View ที่ขึ้นต้นด้วย `_` เป็นชิ้นส่วนที่ View หลักเรียกใช้ เช่น [การ์ดสินค้า](WebApplication1/Views/Shared/_ProductCard.cshtml) และ [เมนูหลังร้าน](WebApplication1/Views/Account/_DashboardSidebar.cshtml)

ถ้าหา **CSS** ให้คัดลอกชื่อ class จาก `.cshtml` แล้วใช้ **Ctrl+Shift+F** ค้นทั้งโปรเจกต์ สไตล์หน้าร้านอยู่ใน [storefront.css](WebApplication1/wwwroot/css/design/storefront.css) ส่วนสไตล์หลังร้านอยู่ใน [dashboard.css](WebApplication1/wwwroot/css/design/dashboard.css) และสไตล์โครงหน้ารวมอยู่ใน [layout.css](WebApplication1/wwwroot/css/design/layout.css)

## อ่านโฟลเดอร์ใน VS Code แบบลงลึก

เปิดโฟลเดอร์ `402` ใน VS Code แล้วกาง **`WebApplication1`** เพื่ออ่านโค้ด LoveCat ส่วน `WebApplication1.Tests` เป็นโปรเจกต์เทสต์ และ `database` เก็บสคริปต์สร้างตารางตามฟีเจอร์ `Cardlucky` กับ `TawanCatShop` เป็นโฟลเดอร์ข้างเคียง เวลาหาโค้ดตามคู่มือนี้ให้เริ่มใน `WebApplication1`

| โฟลเดอร์ใน `WebApplication1` | อ่านอย่างไรและเปิดตอนไหน |
|---|---|
| **`Controllers`** | จุดรับคำขอจาก URL หรือฟอร์ม เมธอดที่รับคำขอเรียกว่า **Action** เช่น `AccountController.OrderStatus` รับตัวกรองจากหน้าออเดอร์ ส่วน `CheckoutController.PlaceOrder` รับคำขอยืนยันสั่งซื้อ |
| **`Views`** | ไฟล์ Razor `.cshtml` ผสม HTML และ C# ตามปกติให้หา `Views/<Controller>/<Action>.cshtml` เช่น `Views/Account/OrderStatus.cshtml` ส่วนไฟล์ชื่อขึ้นต้น `_` มักเป็นชิ้นส่วนใช้ซ้ำ เช่น การ์ดสินค้า |
| **`ViewModels`** | โครงข้อมูลที่ออกแบบให้ฟอร์มหรือหน้าจอใช้โดยเฉพาะ เช่น `DashboardQuery` รับคำค้นและตัวกรอง ฟิลด์ของ ViewModel จึงอาจมาจากหลายตารางหรือเป็นค่าคำนวณ |
| **`Services`** | รวมขั้นตอนที่ระบบลงมือทำ `Catalog` ดูสินค้าและรีวิว, `Orders` คิดราคา สร้างออเดอร์ และจัดส่ง, `Backoffice` จัดการสินค้าและบัญชี, `Dashboards` ประกอบข้อมูลแต่ละ Role, `Accounts` ดูโปรไฟล์และที่อยู่, `Identity` ดูผู้ใช้ปัจจุบัน และ `Storage` เก็บไฟล์ |
| **`Domain`** | รวมกฎที่ใช้อธิบายพฤติกรรม เช่น `OrderWorkflow` กำหนดว่า Role ไหนเปลี่ยนสถานะได้ และ `PayoutStage` ใช้สถานะจัดส่งกับข้อมูลโอนเพื่อหาสถานะการเงิน |
| **`Models` กับ `Data`** | `Models` ประกาศข้อมูล เช่น `Order` และ `OrderShipment` ส่วน `Data` กำหนดวิธีแมปกับ MySQL ผ่าน `AppDbContext` รวมถึง Query ที่ใช้ซ้ำ เช่น การจำกัดข้อมูลตามร้านใน `ShopScope` |
| **`Filters` กับ `Middleware`** | Filter เช่น `RequireRole` ตรวจสิทธิ์ก่อนเข้า Action ส่วน Middleware ทำงานในเส้นทางรับคำขอ เช่น อัปเดตสถานะสมาชิกที่เก็บไว้ในเซสชัน |
| **`ViewComponents` กับ `TagHelpers`** | View Component เตรียมข้อมูลสำหรับส่วนที่ใช้ซ้ำ เช่น Navbar ส่วน Tag Helper ช่วยสร้างลิงก์และฟอร์มตาม Controller/Action หรือกำหนดสถานะเมนูที่กำลังเปิด |
| **`Extensions`, `Options`, `Properties`, `Helpers`** | `Extensions` มีจุดลงทะเบียน DI, `Options` คือคลาสรับค่าตั้งระบบ, `Properties` มี launch profile สำหรับรันบนเครื่อง และ `Helpers` เก็บฟังก์ชันช่วยงานเฉพาะเรื่อง |
| **`wwwroot` กับ `App_Data`** | `wwwroot` เก็บ CSS, JavaScript, ไฟล์ข้อมูลที่หน้าเว็บโหลด และไลบรารีฝั่งเบราว์เซอร์ ส่วน `App_Data/uploads` เก็บรูปที่ผู้ใช้อัปโหลด |
| **`bin` กับ `obj`** | ผลลัพธ์และไฟล์ระหว่าง build ที่ .NET สร้างให้ เมื่อจะดูหรือแก้ต้นฉบับให้เปิดไฟล์จากโฟลเดอร์ข้างบน |

### อ่านหนึ่งคำขอจากหน้าจอถึง MySQL

ใช้ตัวอย่างหน้า **สถานะออเดอร์ → ยังไม่โอน** เพื่อดูว่าข้อมูลไหลผ่านแต่ละชั้นยังไง

1. [OrderStatus.cshtml](WebApplication1/Views/Account/OrderStatus.cshtml) มี dropdown ชื่อ `payout` เมื่อกดค้นหา ฟอร์ม GET ส่งค่าไปที่ `/Account/OrderStatus`
2. [AccountController](WebApplication1/Controllers/AccountController.cs) รับค่าเป็น `DashboardQuery` จาก query string แล้วสร้าง `DashboardRequest` ให้ Dashboard ของฝ่ายบัญชี จุดนี้คือการรับข้อมูลจากหน้าเว็บ
3. [AccountingDashboard](WebApplication1/Services/Dashboards/AccountingDashboard.cs) กรองเลขออเดอร์และสถานะจัดส่งก่อน จากนั้นใช้ [PayoutSummaryReader](WebApplication1/Services/Backoffice/PayoutSummaryReader.cs) นับทุกสถานะสำหรับ dropdown และรวมยอดเฉพาะพร้อมโอน
4. [PayoutQueries](WebApplication1/Data/PayoutQueries.cs) ใช้ `WherePayout` เลือกออเดอร์ที่ส่งถึงแล้ว และสำหรับ `Unpaid` เลือกกรณียังไม่มีรายการอนุมัติหรือยังไม่มีวันที่โอน เงื่อนไขอยู่ใน `IQueryable` จึงถูกส่งให้ EF Core แปลเป็น SQL ก่อนแบ่งหน้า
5. เมื่อเตรียมข้อมูลเสร็จ `AccountingOrderStatusModel` กลับไปให้ View วาดตาราง แสดงยอดสรุป และทำให้ dropdown ยังเลือกค่าเดิมอยู่

**ถ้าอาจารย์ถามว่าทำไมกรองก่อนแบ่งหน้า** ตอบได้ว่า ถ้าแบ่งหน้าก่อน ระบบจะค้นเฉพาะออเดอร์ที่หลุดเข้ามาในหน้านั้น การกรองก่อนทำให้ยอดรวมและจำนวนหน้าคิดจากผลค้นหาทั้งหมด

### อ่านปุ่มบวกทีละไฟล์

1. [_ProductCard.cshtml](WebApplication1/Views/Shared/_ProductCard.cshtml) วางปุ่มที่มี `data-add-to-cart` และข้อมูลสินค้าบนการ์ด
2. [main.js](WebApplication1/wwwroot/js/app/main.js) จับการคลิกแล้วเรียก `cart.add` ส่วน [cart.js](WebApplication1/wwwroot/js/app/cart.js) เก็บจำนวนในเบราว์เซอร์
3. [drawers.js](WebApplication1/wwwroot/js/app/drawers.js) อัปเดตไอคอนตะกร้าและเรียก [pricing.js](WebApplication1/wwwroot/js/app/pricing.js) ขอราคาปัจจุบันจาก `CheckoutController.Quote`
4. การกดยืนยันสั่งซื้อจะเข้า `CheckoutController.PlaceOrder` และ [CheckoutService](WebApplication1/Services/Orders/CheckoutService.cs) เพื่อบันทึกออเดอร์ จัดกลุ่มตามร้าน และตัดสต็อก ณ ราคาที่ตรวจแล้ว

**ถ้าอาจารย์ถามว่ากดบวกแล้วบันทึกอะไร** ตอบได้ว่า กดบวกจะเพิ่มรายการในตะกร้าของเบราว์เซอร์และขอเช็กราคาจากเซิร์ฟเวอร์ ส่วนออเดอร์กับสต็อกจะถูกบันทึกตอนยืนยันสั่งซื้อสำเร็จ

## เริ่มใช้งานบนเครื่อง

ต้องมี **.NET 10 SDK** และ **MySQL Server พร้อม MySQL Command Line Client** เปิด PowerShell จากโฟลเดอร์โปรเจกต์

1. สำหรับเครื่องที่ยังไม่มีตาราง LoveCat ให้ใช้ [ไฟล์ส่งออกฐานข้อมูลปัจจุบัน](csi402db-current-20261125-040032.sql) ซึ่งมีคำสั่งสร้างและเลือกฐาน `csi402db` ไฟล์นี้มีข้อมูลลูกค้าและสลิป เก็บไว้ใช้ส่วนตัว

   ```powershell
   mysql -u root -p --default-character-set=utf8mb4
   ```

   จากนั้นพิมพ์ `SOURCE C:/Users/PC/Desktop/402/csi402db-current-20261125-040032.sql;` ใน MySQL Command Line โดยปรับ path ให้ตรงกับเครื่อง หากมีฐานที่ใช้งานอยู่แล้ว ให้เชื่อมฐานเดิมในขั้นที่ 2 สำหรับ MonsterASP ให้เลือกฐานเปล่าที่โฮสต์จัดไว้ แล้วนำเข้า [ไฟล์ฉบับปรับสำหรับ MonsterASP](csi402db-monsterasp.sql) ผ่าน phpMyAdmin สคริปต์แยกตามฟีเจอร์อยู่ใน [database/](database/) โดยไฟล์ `12_order_shipments.sql` สร้างตารางเลขพัสดุ

2. ตั้งค่า connection string ใน PowerShell ให้ตรงกับบัญชีและพอร์ตของ MySQL บนเครื่อง

   ```powershell
   $env:ConnectionStrings__DefaultConnection = 'Server=localhost;Port=3306;Database=csi402db;Uid=<ผู้ใช้ MySQL>;Pwd=<รหัสผ่าน MySQL>;CharSet=utf8mb4;'
   dotnet watch run --project WebApplication1 --launch-profile http
   ```

3. เปิด [หน้าแรก LoveCat](http://localhost:5173/) จากเบราว์เซอร์

## บัญชีสาธิต

ชื่อต่อไปนี้เป็นตัวอย่างบัญชีจากชุดสาธิตเดิม ให้ใช้ชื่อบัญชีตามข้อมูลของฐานที่เว็บกำลังเชื่อมต่อ

| Role | ชื่อผู้ใช้ | หน้าที่ |
|---|---|---|
| User | `demo_user` | ซื้อสินค้า จัดการที่อยู่ ดูคูปองและเงินสะสม ติดตามออเดอร์ และรีวิวสินค้าที่ได้รับแล้ว |
| Shop | `demo_shop_a`, `demo_shop_b` | จัดการสินค้าและสต็อกของร้าน แพ็กของ บันทึกส่งพัสดุ ตอบลูกค้าและรีวิว ดูยอดและหลักฐานโอนของร้าน |
| Raider | `demo_raider` | รับงานในจังหวัดที่ได้รับมอบหมาย ดูจุดรับสินค้า และบันทึกว่าส่งถึงแล้ว |
| Accounting | `demo_accounting` | ตรวจสลิป ขอสลิปใหม่ ยกเลิกยอดที่ยังไม่ยืนยัน อนุมัติและบันทึกโอนให้ร้าน |
| Admin | `admin` | จัดการผู้ใช้ ผูกบัญชีกับร้านหรือพื้นที่ไรเดอร์ และตั้งคูปองกับ Flash Sale |
| Owner | `demo_owner` | ดูภาพรวมรายได้แพลตฟอร์มและจัดการสิทธิ์ Admin |

## วิธีเปิดดูฐานข้อมูล

ดูฐานที่เว็บกำลังใช้โดยเปิด **MySQL Command Line Client** แล้วเข้าสู่ระบบด้วยบัญชี MySQL ที่ตั้งไว้ใน `ConnectionStrings__DefaultConnection` เลือกฐาน `csi402db` ถ้าเว็บใช้ชื่อฐานอื่น ให้เลือกชื่อตาม connection string คำสั่งด้านล่างใช้ดูข้อมูลอย่างเดียว

```sql
USE csi402db;
SHOW TABLES;
DESCRIBE order_shops;
```

ถ้าใช้ **MySQL Workbench** ให้เปิดการเชื่อมต่อเดียวกับเว็บ จากนั้นไปที่ **Schemas → csi402db → Tables** คลิกขวาที่ตารางแล้วเลือก **Select Rows – Limit 1000**

| อยากดูอะไร | เปิดตารางไหน |
|---|---|
| ร้านไหนเป็นเจ้าของสินค้า | `products` → `product_shops` → `marketplace_shops` |
| ลูกค้าจ่ายรวมเท่าไหร่และตรวจสลิปแล้วหรือยัง | `basket_payments` ดูยอดรวมและสถานะการชำระเงิน |
| ยอดชำระเดียวแยกเป็นออเดอร์ของร้านไหนบ้าง | `order_shops` เชื่อม `payment_id` กับ `order_id` และ `shop_id` |
| แต่ละออเดอร์ซื้ออะไรและส่งถึงหรือยัง | `orders` ดูสถานะจัดส่ง ส่วน `order_items` เก็บรายการสินค้าและราคา ณ ตอนซื้อ |
| ร้านส่งกับขนส่งไหนและเลขอะไร | `order_shipments` เชื่อมกับออเดอร์ด้วย `order_id` |
| ร้านได้สุทธิเท่าไหร่และโอนแล้วหรือยัง | `shop_payouts` ดู `net_amount` และ `transfer_date` ถ้ามียอดอนุมัติแต่วันที่ยังว่าง แปลว่าพร้อมโอน |

### ไล่ดูออเดอร์จากสองร้านด้วยตัวอย่างสาธิต

ในตัวอย่างสาธิต `DEMO-AWAIT` กับ `DEMO-SHIPPING` เป็นออเดอร์จากตะกร้าเดียวกัน หากใช้ฐานปัจจุบัน ให้เลือกออเดอร์สองร้านจากตะกร้าเดียวกันแทน เปิดตาราง `orders` แล้วใช้ `id` ของแต่ละแถวไปดู `order_shops.order_id` ทั้งคู่จะมี **`payment_id` เหมือนกัน แต่ `shop_id` ต่างกัน** จากนั้นเปิด `basket_payments` ด้วยรหัสชำระเงินนั้นเพื่อดูยอดรวม

กลับมาดู `orders.status` จะเห็นว่า `DEMO-AWAIT` ส่งถึงแล้ว ส่วน `DEMO-SHIPPING` ยังกำลังจัดส่ง จึงอนุมัติยอดโอนได้เฉพาะรายการแรก สำหรับตัวอย่างข้อมูลการโอน ให้เทียบ `DEMO-READY` กับ `DEMO-PAID` ใน `shop_payouts` โดยรายการแรกยังไม่มี `transfer_date` ส่วนรายการหลังมีวันที่โอนแล้ว

## Flow การทำงานตั้งแต่ User ถึง Owner

**Admin ตั้งค่าร้านและพื้นที่ไรเดอร์ → Shop ลงสินค้า → User สั่งซื้อและแนบสลิป → Accounting ตรวจสลิป → Shop จัดส่ง → Raider หรือ User ยืนยันว่าของถึง → Accounting อนุมัติและบันทึกโอนแยกตามร้าน → Owner ดูภาพรวม**

1. **ลูกค้าสั่งซื้อ** ระบบคำนวณราคา Flash Sale ก่อน ตามด้วยคูปองและเงินสะสมที่ลูกค้าเลือกใช้ ลูกค้าจ่ายยอดรวมพร้อมเพย์และแนบสลิปใบเดียว ระบบสร้างหนึ่งออเดอร์ต่อร้านในตะกร้า
2. **ฝ่ายบัญชีตรวจสลิป** เมื่อพบเงินเข้าจึงยืนยันการชำระเงิน หากหลักฐานมีปัญหาให้ขอสลิปใหม่ได้ หากตรวจแล้วไม่พบเงินเข้า สามารถยกเลิกยอดที่ยังไม่ยืนยันทั้งตะกร้า ระบบคืนสต็อก คูปอง เงินสะสม และโควตา Flash Sale ที่มีบันทึกการจอง
3. **ร้านจัดส่ง** เมื่อสลิปผ่าน ร้านเตรียมสินค้า ปลายทางกรุงเทพฯ เข้าคิวไรเดอร์ที่ได้รับมอบหมาย ปลายทางต่างจังหวัดให้ร้านบันทึกบริษัทขนส่งและเลขติดตาม ลูกค้ากดยืนยันเมื่อได้รับของ การบันทึกเลขพัสดุทำครั้งเดียวต่อออเดอร์
4. **ฝ่ายบัญชีอนุมัติแยกแต่ละร้าน** เมื่อออเดอร์ส่งถึงแล้วจึงอนุมัติยอดสุทธิ จากนั้นบันทึกวันโอน เลขอ้างอิง และสลิป หน้า **สถานะออเดอร์** กรอง **ยังไม่โอน** ได้ ซึ่งรวม **รออนุมัติ** กับ **พร้อมโอน** กล่องยอดที่ต้องโอนรวมเฉพาะ **พร้อมโอน**
5. **หลังส่งถึง** ลูกค้ารีวิวและได้รับยอดซื้อสะสมจากค่าสินค้าสุทธิที่จ่ายจริง ทุกยอดสะสมครบ 1,000 บาทได้เครดิต 1 บาท ส่วน Owner ดูภาพรวมค่าคอมและต้นทุนโปรโมชั่นของ LoveCat

**ตัวอย่างสาธิตบนเว็บ** ในหน้าสถานะออเดอร์ เลือกสองออเดอร์ที่จ่ายรวมกันและสาธิตให้รายการหนึ่งส่งถึงก่อน ฝ่ายบัญชีจะอนุมัติได้เฉพาะรายการที่ส่งถึงแล้ว จากนั้นใช้ตัวกรอง **ยังไม่โอน** และ **โอนแล้ว** ดูผลหลังบันทึกโอน

## หาโค้ดตามฟีเจอร์

| ฟีเจอร์ | จุดที่ควรเปิด |
|---|---|
| ราคา คูปอง Flash Sale และเงินสะสม | [PricingService](WebApplication1/Services/Orders/PricingService.cs), [CouponCalculator](WebApplication1/Domain/Pricing/CouponCalculator.cs), [LoyaltyRules](WebApplication1/Domain/Pricing/LoyaltyRules.cs) |
| สร้างยอดชำระรวมและแยกออเดอร์ตามร้าน | [CheckoutController](WebApplication1/Controllers/CheckoutController.cs), [CheckoutService](WebApplication1/Services/Orders/CheckoutService.cs) |
| ตรวจสลิปและยกเลิกทั้งตะกร้า | [PaymentsController](WebApplication1/Controllers/PaymentsController.cs), [BasketCancellationService](WebApplication1/Services/Orders/BasketCancellationService.cs) |
| เปลี่ยนสถานะและบันทึกเลขพัสดุ | [OrderWorkflow](WebApplication1/Domain/Orders/OrderWorkflow.cs), [OrderFulfillmentService](WebApplication1/Services/Orders/OrderFulfillmentService.cs), [ShipmentService](WebApplication1/Services/Orders/ShipmentService.cs) |
| แยกสิทธิ์ข้อมูลตามร้าน | [ShopScope](WebApplication1/Data/ShopScope.cs), [RequireRoleAttribute](WebApplication1/Filters/RequireRoleAttribute.cs) |
| ค่าคอม ยอดโอน และตัวกรอง | [AccountingService](WebApplication1/Services/Backoffice/AccountingService.cs), [PayoutStage](WebApplication1/Domain/Accounting/PayoutStage.cs), [PayoutQueries](WebApplication1/Data/PayoutQueries.cs), [หน้าสถานะออเดอร์](WebApplication1/Views/Account/OrderStatus.cshtml) |
| รีวิว การติดตามร้าน และข้อความ | [ReviewService](WebApplication1/Services/Catalog/ReviewService.cs), [ShopInteractions](WebApplication1/Services/Backoffice/ShopInteractions.cs) |
| การแมปตาราง MySQL | [AppDbContext](WebApplication1/Data/AppDbContext.cs), [MarketplaceMapping](WebApplication1/Data/MarketplaceMapping.cs), [ShippingMapping](WebApplication1/Data/ShippingMapping.cs) |

### เหตุผลที่แบ่งโค้ดแบบนี้

- **MVC และ Dependency Injection** แยกการรับคำขอ การประมวลผล และการแสดงผล Controller รับบริการผ่าน DI ที่ลงทะเบียนใน [ServiceCollectionExtensions](WebApplication1/Extensions/ServiceCollectionExtensions.cs)
- **Strategy** ให้แต่ละ Role ประกอบ Dashboard ของตัวเองผ่าน `IRoleDashboard` และให้ `AccountController` เลือกตามบทบาทปัจจุบัน
- **State Machine** ใช้ `OrderWorkflow` จำกัดการเปลี่ยนสถานะ ส่วน `PayoutStages` แปลงสถานะจัดส่งและข้อมูลโอนเป็นสถานะการเงินที่ใช้บนหน้าเว็บ
- **ธุรกรรมและการอัปเดตแบบมีเงื่อนไข** รวมการจองสต็อก โควตา และบันทึกออเดอร์เป็นชุดเดียว และตรวจสถานะเดิมทุกครั้งก่อนอัปเดต เพื่อป้องกันการกดซ้ำหรือกดชนกัน
- **Tag Helper และ Post/Redirect/Get** ช่วยให้ฟอร์มกับลิงก์อ้างอิง Controller และ Action ตาม route หลังบันทึกฟอร์ม ระบบจึงกลับไปหน้าที่อ่านซ้ำได้ด้วย GET

## ทดสอบ

```powershell
dotnet run --project WebApplication1.Tests -c Release
node --preserve-symlinks-main WebApplication1.Tests/checkout-ui.test.mjs
```

ชุดทดสอบ C# อยู่ใน [Program.cs](WebApplication1.Tests/Program.cs) ใช้ตัวรันของโปรเจกต์และ SQLite in-memory สำหรับเทสต์ที่ต้องใช้ฐานข้อมูล ชุดทดสอบ JavaScript แยกตรวจการแสดงยอดเมื่อเปิดและปิดการใช้เงินสะสม

ถ้าจะตรวจ Query กับ MySQL ให้เพิ่ม `-- --mysql-read-only WebApplication1/appsettings.Development.json` ต่อท้ายคำสั่ง C# โดยชี้ไฟล์ตั้งค่าไปยังฐานที่ต้องการตรวจ เทสต์โหมดนี้อ่านข้อมูลจาก MySQL เพื่อเช็กการแปล Query ของ EF Core

## ที่เก็บรูปและเครดิต

รูปที่อัปโหลดเก็บใน `WebApplication1/App_Data/uploads` และแสดงผ่าน `/uploads` ส่วนรูปสินค้าในข้อมูลสาธิตอ้างอิงจาก [Unsplash](https://unsplash.com) ไอคอนใช้ [Bootstrap Icons](https://icons.getbootstrap.com) และกราฟใช้ [Chart.js](https://www.chartjs.org)

## โครงสร้างเว็บ LoveCat

LoveCat ใช้ **ASP.NET Core MVC** ฝั่งเบราว์เซอร์แสดงหน้า Razor และจัดการการคลิก ส่วนฝั่งเซิร์ฟเวอร์ตรวจสิทธิ์ ประมวลผลกฎธุรกิจ และอ่านหรือบันทึกข้อมูลผ่าน EF Core ไปที่ MySQL

```text
402/
├── WebApplication1/        เว็บที่เปิดใช้งาน
│   ├── Program.cs          จุดเริ่มต้นของแอปและเส้นทางรับคำขอ
│   ├── Controllers/        รับ URL และค่าที่ส่งจากฟอร์ม
│   ├── Services/           ทำงานตามฟีเจอร์ เช่น สั่งซื้อ จัดส่ง และบัญชี
│   ├── Domain/             กฎธุรกิจ เช่น เปลี่ยนสถานะและคำนวณส่วนลด
│   ├── Models/             โครงข้อมูล เช่น สินค้า ออเดอร์ และยอดโอน
│   ├── Data/               แมปโมเดลกับ MySQL และประกอบ Query
│   ├── ViewModels/         ข้อมูลสำหรับฟอร์มและหน้าจอแต่ละหน้า
│   ├── Views/              หน้า Razor และส่วนที่ใช้ซ้ำ
│   ├── wwwroot/            CSS, JavaScript และไฟล์ข้อมูลที่หน้าเว็บโหลด
│   └── App_Data/uploads/   รูปที่ผู้ใช้อัปโหลด
├── WebApplication1.Tests/  ชุดทดสอบกฎธุรกิจและการอ่านข้อมูล
└── database/               สคริปต์สร้างตาราง MySQL ตามฟีเจอร์
```

### คำขอวิ่งผ่านส่วนไหนบ้าง

```text
เบราว์เซอร์ → Routing → ตรวจสิทธิ์ → Controller → Service
                                                 │
                                กฎจาก Domain + อ่าน/เขียนผ่าน EF Core → MySQL
                                                 │
เบราว์เซอร์ ← HTML จาก View หรือ JSON ← ผลลัพธ์ที่ Controller คืน
```

1. **Routing และสิทธิ์** [Program.cs](WebApplication1/Program.cs) กำหนดวิธีจับคู่ URL กับ Action และลำดับ middleware ส่วน [RequireRole](WebApplication1/Filters/RequireRoleAttribute.cs) กำหนดว่า Role ไหนเรียก Action ที่จำกัดสิทธิ์ได้
2. **Controller** อ่านค่าจากคำขอและส่งต่อไปยังบริการ เช่น [CheckoutController](WebApplication1/Controllers/CheckoutController.cs) รับข้อมูลตะกร้าหรือคำขอสั่งซื้อ
3. **Service และ Domain** บริการจัดลำดับขั้นตอนและดึงข้อมูล เช่น [PricingService](WebApplication1/Services/Orders/PricingService.cs) อ่านราคากับสต็อก แล้วเรียก [CouponCalculator](WebApplication1/Domain/Pricing/CouponCalculator.cs) คำนวณส่วนลดตามกฎ
4. **EF Core และ MySQL** [AppDbContext](WebApplication1/Data/AppDbContext.cs) เป็นจุดที่บริการใช้เข้าถึงข้อมูล เงื่อนไขค้นหาถูกแปลเป็น SQL ส่วนการบันทึกออเดอร์ใช้ธุรกรรมเพื่อให้รายการและสต็อกสอดคล้องกัน
5. **ผลลัพธ์กลับไปหน้าเว็บ** Controller ส่ง ViewModel ให้ Razor เมื่อเปิดหน้า เช่น [OrderStatus.cshtml](WebApplication1/Views/Account/OrderStatus.cshtml) ส่วนการขอราคาในตะกร้าใช้ JSON ให้ JavaScript อัปเดตยอดบนหน้าที่เปิดอยู่

**เวลาอาจารย์ชี้เว็บให้เปิดโค้ด** ให้เริ่มจาก URL เพื่อหา Controller และ View ถ้าถามว่าปุ่มกดแล้วตรวจอะไร ให้ตามชื่อ Action ไปยัง Service และกฎหรือ Query ที่เมธอดนั้นเรียก

## ติวฟังก์ชัน C# ที่ใช้จริง

ใน C# ฟังก์ชันที่อยู่ในคลาสเรียกว่า **เมธอด** วิธีอ่านคือดูชื่อเมธอดก่อน แล้วถามว่า **ใครเรียก → ส่งค่าอะไรมา → เมธอดทำอะไร → คืนอะไร** ค้นด้วยชื่อเมธอดพร้อมวงเล็บเปิด เช่น `PlaceOrderAsync(` เพื่อเจอทั้งจุดประกาศและจุดเรียก

### ฟังก์ชันที่ทำงานเมื่อกดค้นหา “ยังไม่โอน”

| เมธอด | รับอะไร | ทำอะไรและคืนอะไร |
|---|---|---|
| [AccountController.OrderStatus](WebApplication1/Controllers/AccountController.cs#L93) | `DashboardQuery` จาก URL เช่น `payout=Unpaid` | ส่งคำขอไป `DashboardAsync` เพื่อเตรียมหน้าสถานะออเดอร์ |
| [AccountController.DashboardAsync](WebApplication1/Controllers/AccountController.cs#L134) | ชื่อหน้ากับตัวกรอง | เลือก Dashboard ตาม Role แล้วคืนผลสำหรับแสดง View หากหน้าไม่มีในบทบาทนั้นจะคืน 404 |
| [AccountingDashboard.OrderStatusAsync](WebApplication1/Services/Dashboards/AccountingDashboard.cs#L111) | คำขอที่มีเลขออเดอร์ สถานะจัดส่ง สถานะการเงิน และเลขหน้า | เตรียมยอดสรุป กรองรายการ แบ่งหน้า แล้วคืน `DashboardPage` ที่ View ใช้แสดงผล |
| [PayoutQueries.WherePayout](WebApplication1/Data/PayoutQueries.cs#L10) | คำสั่งค้นหาออเดอร์ `IQueryable<Order>` กับตัวเลือก `PayoutFilter` | ต่อเงื่อนไขการเงินและคืน Query ให้เรียกต่อได้ สำหรับ `Unpaid` จะเลือกออเดอร์ที่ส่งถึงและยังรออนุมัติหรือยังไม่มีวันที่โอน |
| [PayoutSummaryReader.ReadAsync](WebApplication1/Services/Backoffice/PayoutSummaryReader.cs#L15) | Query ที่กรองเลขออเดอร์และสถานะจัดส่งแล้ว | คืน `PayoutSummary` ที่มีจำนวนแต่ละสถานะและยอดสุทธิของรายการพร้อมโอน ก่อนนำ dropdown สถานะการเงินไปกรองตาราง |

**จุดจำ:** `WherePayout` คืนเงื่อนไขค้นหา ส่วน `CountAsync` หรือการดึงรายการเป็นจุดที่ EF Core ส่งคำสั่งไปฐานข้อมูล

### ฟังก์ชันที่ทำงานตั้งแต่คิดราคาถึงโอนให้ร้าน

| เมธอด | ใครเรียกและส่งอะไรมา | ทำอะไรและคืนอะไร |
|---|---|---|
| [PricingService.QuoteAsync](WebApplication1/Services/Orders/PricingService.cs#L30) | ตะกร้าหรือ Checkout ส่งรหัสสินค้า จำนวน คูปอง และตัวเลือกเงินสะสม | อ่านราคากับสต็อกจากฐาน คำนวณ Flash Sale คูปอง เงินสะสมและค่าส่ง แล้วคืน `PricingQuote` |
| [CheckoutService.PlaceOrderAsync](WebApplication1/Services/Orders/CheckoutService.cs#L39) | `CheckoutController` ส่งรหัสลูกค้าที่ล็อกอินและคำขอสั่งซื้อ | ตรวจข้อมูล คำนวณยอดอีกครั้ง เทียบยอดที่ลูกค้ายืนยัน และตรวจคำขอซ้ำ คืนเลขออเดอร์และยอดชำระเมื่อสำเร็จ หรือข้อความเมื่อทำต่อไม่ได้ |
| [CheckoutService.SaveOrderAsync](WebApplication1/Services/Orders/CheckoutService.cs#L97) | `PlaceOrderAsync` ส่งยอดคำนวณแล้ว ที่อยู่ และหลักฐานชำระเงิน | ตัดสต็อกและโควตา สร้างยอดชำระรวม แยกออเดอร์ด้วย `ShopId` และบันทึกสิทธิ์ที่ใช้ในธุรกรรมเดียว |
| [BasketCancellationService.CancelAsync](WebApplication1/Services/Orders/BasketCancellationService.cs#L11) | ฝ่ายบัญชีส่งรหัสยอดชำระ ผู้ดำเนินการ และเหตุผล | ยกเลิกยอดที่ยังไม่ยืนยันทั้งตะกร้า คืนสต็อกและสิทธิ์ที่เกี่ยวข้อง แล้วคืนผลสำเร็จหรือข้อความปฏิเสธ |
| [ShipmentService.DispatchAsync](WebApplication1/Services/Orders/ShipmentService.cs#L28) | ร้านส่งเลขออเดอร์ ขนส่ง และเลขติดตาม | ตรวจว่าออเดอร์เป็นของร้านและพร้อมส่งพัสดุ จากนั้นบันทึกข้อมูลขนส่งพร้อมเปลี่ยนสถานะเป็นกำลังจัดส่ง |
| [AccountingService.ApprovePayoutAsync](WebApplication1/Services/Backoffice/AccountingService.cs#L189) | ฝ่ายบัญชีส่งรหัสออเดอร์ | ตรวจว่าส่งถึงแล้ว คำนวณค่าคอมและยอดสุทธิ แล้วบันทึกยอดที่อนุมัติ |
| [AccountingService.RecordTransferAsync](WebApplication1/Services/Backoffice/AccountingService.cs#L234) | ฝ่ายบัญชีส่งรหัสออเดอร์ วันที่ เลขอ้างอิง และสลิป | ตรวจยอดที่อนุมัติแล้วและยังไม่มีวันที่โอน แล้วบันทึกหลักฐานการโอนให้ร้าน |

### อ่านชนิดค่าที่อยู่หน้าชื่อเมธอด

- `Task<PricingQuote>` คือผลคำนวณราคาที่ต้อง `await` ก่อนนำไปใช้
- `Task<OperationResult<PlacedOrder>>` คือผลสั่งซื้อที่ต้องรอ แล้วตรวจ `Succeeded` เพื่อรู้ว่าจะใช้ข้อมูลใน `Value` หรืออ่านข้อผิดพลาดจาก `Error`
- `IQueryable<Order>` คือ Query ที่ยังต่อเงื่อนไขได้ เช่น ต่อ `Where` ก่อนสั่งนับหรือดึงข้อมูล
- `CancellationToken` ใช้ส่งสัญญาณให้หยุดงานที่กำลังรอ เช่น เมื่อคำขอ HTTP ถูกยกเลิก

**ซ้อมตอบ:** ถ้าอาจารย์ถามว่ากดค้นหาแล้วเขียนลงฐานหรือไม่ ให้เปิด `WherePayout` แล้วอธิบายว่าเมธอดนี้เติมเงื่อนไขเพื่ออ่านรายการ ส่วนการบันทึกว่าโอนแล้วเกิดใน `RecordTransferAsync`

<!-- csharp-method-index:start -->
## สารบัญฟังก์ชัน C# ทั้งโปรเจกต์

รวม **432 รายการใน 108 ไฟล์**

กางโฟลเดอร์แล้วกดลิงก์ชื่อฟังก์ชันเพื่อไปยังบรรทัดประกาศ ใช้ **Ctrl+F** ค้นชื่อเมธอด ส่วนหลังลูกศรคือชนิดค่าที่เมธอดคืน ส่วนในวงเล็บคือพารามิเตอร์ แต่ละรายการมีคำอธิบายหน้าที่เป็นภาษาไทย

รายการนี้ครอบคลุมเมธอด ตัวสร้างออบเจ็กต์ และฟังก์ชันย่อยที่ประกาศชื่อในไฟล์ C# ของเว็บและเทสต์ ส่วน property, lambda ที่ไม่มีชื่อ และไฟล์ที่สร้างระหว่าง build อยู่นอกสารบัญ ดูตัวอย่างการอธิบายหน้าที่และการเรียกต่อได้ในหัวข้อ **ติวฟังก์ชัน C# ที่ใช้จริง** ด้านบน

<details>
<summary><code>WebApplication1.Tests</code></summary>

- **[Program.cs](WebApplication1.Tests/Program.cs)**
  - [`File`](WebApplication1.Tests/Program.cs#L74) `(int size) → Microsoft.AspNetCore.Http.FormFile` — สร้างข้อมูลหรือออบเจ็กต์สำหรับจัดฉากการทดสอบ
  - [`NewShippingDbAsync`](WebApplication1.Tests/Program.cs#L111) `() → Task<(SqliteConnection Connection, AppDbContext Db)>` — สร้างข้อมูลหรือออบเจ็กต์สำหรับจัดฉากการทดสอบ
  - [`Parcel`](WebApplication1.Tests/Program.cs#L130) `(int id, string no, OrderStatus status, int shop = 1, bool rider = false, BasketPayment? payment = null) → Order` — สร้างข้อมูลหรือออบเจ็กต์สำหรับจัดฉากการทดสอบ
  - [`NewShipmentService`](WebApplication1.Tests/Program.cs#L144) `(AppDbContext db, string role = Roles.Shop, DateTime? now = null) → ShipmentService` — สร้างข้อมูลหรือออบเจ็กต์สำหรับจัดฉากการทดสอบ
  - [`Ship`](WebApplication1.Tests/Program.cs#L148) `(string carrier = "flash", string tracking = "TH12345678") → ShipmentDetails` — สร้างข้อมูลหรือออบเจ็กต์สำหรับจัดฉากการทดสอบ
  - [`CheckBasketCancellation`](WebApplication1.Tests/Program.cs#L151) `(AppDbContext db) → Task` — ทดสอบยกเลิกตะกร้าหลายร้านและตรวจการคืนสิทธิ์ครั้งเดียว
  - [`Digits`](WebApplication1.Tests/Program.cs#L748) `(int length) → string` — สร้างข้อความตัวเลขตามความยาวที่ต้องการสำหรับเทสต์
  - [`NewController`](WebApplication1.Tests/Program.cs#L972) `() → ShipmentsController` — สร้างข้อมูลหรือออบเจ็กต์สำหรับจัดฉากการทดสอบ
  - [`Run`](WebApplication1.Tests/Program.cs#L1121) `(string name, Func<Task> test) → Task` — เรียกเคสทดสอบและบันทึกผลผ่านหรือไม่ผ่าน
  - [`Assert`](WebApplication1.Tests/Program.cs#L1136) `(bool condition, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(condition))] string? expression = null) → void` — ตรวจเงื่อนไขที่เทสต์คาดไว้และแจ้งข้อผิดพลาดเมื่อไม่ตรง
  - [`TestClock.GetUtcNow`](WebApplication1.Tests/Program.cs#L1143) `() → DateTimeOffset` — คืนเวลาที่กำหนดให้การทดสอบทำซ้ำได้
  - [`TestOrderNumbers.NextAsync`](WebApplication1.Tests/Program.cs#L1149) `(CancellationToken cancellationToken = default) → Task<string>` — ค้นคำขอเดิมหรือตรวจและสร้างเลขออเดอร์ที่ใช้งานได้
  - [`EmptyTempDataProvider.LoadTempData`](WebApplication1.Tests/Program.cs#L1154) `(Microsoft.AspNetCore.Http.HttpContext context) → IDictionary<string, object>` — จำลองการอ่านหรือบันทึกข้อความชั่วคราวในชุดทดสอบ
  - [`EmptyTempDataProvider.SaveTempData`](WebApplication1.Tests/Program.cs#L1155) `(Microsoft.AspNetCore.Http.HttpContext context, IDictionary<string, object> values) → void` — จำลองการอ่านหรือบันทึกข้อความชั่วคราวในชุดทดสอบ
  - [`TestAccount.IsInRole`](WebApplication1.Tests/Program.cs#L1166) `(params string[] roles) → bool` — ตรวจว่าบทบาทของบัญชีตรงกับเงื่อนไขที่กำหนด
  - [`TestAccount.RequireUserId`](WebApplication1.Tests/Program.cs#L1167) `() → int` — อ่านรหัสผู้ใช้ที่ล็อกอินสำหรับคำขอปัจจุบัน

</details>

<details>
<summary><code>WebApplication1/Controllers</code></summary>

- **[AccountController.cs](WebApplication1/Controllers/AccountController.cs)**
  - [`AccountController.AccountController`](WebApplication1/Controllers/AccountController.cs#L27) `(ICurrentUserService currentUser, IEnumerable<IRoleDashboard> dashboards, IProfileService profiles, IAddressService addresses, AppDbContext db) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ AccountController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`AccountController.Index`](WebApplication1/Controllers/AccountController.cs#L37) `(CancellationToken cancellationToken) → Task<IActionResult>` — เปิดหน้าหลักของส่วนนี้และเตรียมข้อมูลให้ View
  - [`AccountController.Product`](WebApplication1/Controllers/AccountController.cs#L41) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — อ่านรายการสินค้าโดยใช้เงื่อนไขของหน้าปัจจุบัน
  - [`AccountController.Packing`](WebApplication1/Controllers/AccountController.cs#L45) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมรายการออเดอร์ที่ร้านต้องจัดเตรียมและแพ็ก
  - [`AccountController.Shipped`](WebApplication1/Controllers/AccountController.cs#L49) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — แสดงออเดอร์ของร้านที่เริ่มส่งแล้ว
  - [`AccountController.Revenue`](WebApplication1/Controllers/AccountController.cs#L53) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมรายได้และสถานะยอดโอนของร้าน
  - [`AccountController.Tracking`](WebApplication1/Controllers/AccountController.cs#L57) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เปิดส่วนติดตามความคืบหน้าการจัดส่งของลูกค้า
  - [`AccountController.Orders`](WebApplication1/Controllers/AccountController.cs#L61) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เปิดรายการคำสั่งซื้อของลูกค้า
  - [`AccountController.Coupons`](WebApplication1/Controllers/AccountController.cs#L65) `(CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมรายการคูปองที่เก็บไว้และประวัติคูปอง
  - [`AccountController.FlashSales`](WebApplication1/Controllers/AccountController.cs#L69) `(CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมฟอร์มและรายการแคมเปญ Flash Sale
  - [`AccountController.Rewards`](WebApplication1/Controllers/AccountController.cs#L74) `(CancellationToken cancellationToken) → Task<IActionResult>` — เปิดหน้าดูเงินสะสมของลูกค้า
  - [`AccountController.Jobs`](WebApplication1/Controllers/AccountController.cs#L79) `(CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมงานที่ไรเดอร์มีสิทธิ์รับในพื้นที่รับผิดชอบ
  - [`AccountController.MyDeliveries`](WebApplication1/Controllers/AccountController.cs#L83) `(CancellationToken cancellationToken) → Task<IActionResult>` — แสดงงานที่ไรเดอร์คนปัจจุบันรับและกำลังจัดส่ง
  - [`AccountController.History`](WebApplication1/Controllers/AccountController.cs#L87) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมประวัติงานจัดส่งและรายได้ของไรเดอร์
  - [`AccountController.OrderStatus`](WebApplication1/Controllers/AccountController.cs#L91) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เปิดหรือเตรียมตารางสถานะออเดอร์และตัวกรองฝ่ายบัญชี
  - [`AccountController.Commission`](WebApplication1/Controllers/AccountController.cs#L96) `(CancellationToken cancellationToken) → Task<IActionResult>` — เปิดหรืออ่านรายการออเดอร์ส่งถึงแล้วที่รอหักค่าคอม
  - [`AccountController.Ledger`](WebApplication1/Controllers/AccountController.cs#L100) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เปิดหรือรวบรวมรายการบัญชีในช่วงวันที่ที่เลือก
  - [`AccountController.Sales`](WebApplication1/Controllers/AccountController.cs#L104) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมยอดขายและข้อมูลกราฟตามช่วงเวลา
  - [`AccountController.Finance`](WebApplication1/Controllers/AccountController.cs#L108) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมภาพรวมรายได้แพลตฟอร์มและยอดที่เกี่ยวข้อง
  - [`AccountController.Ranking`](WebApplication1/Controllers/AccountController.cs#L112) `(CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมข้อมูลสินค้าขายดีตามเกณฑ์จัดอันดับ
  - [`AccountController.AdminAccess`](WebApplication1/Controllers/AccountController.cs#L116) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — อ่านรายการสำหรับจัดการสิทธิ์แอดมิน
  - [`AccountController.Shops`](WebApplication1/Controllers/AccountController.cs#L121) `(CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมรายชื่อร้านและบัญชีที่ใช้มอบหมายร้านหรือพื้นที่
  - [`AccountController.Users`](WebApplication1/Controllers/AccountController.cs#L126) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — ค้นหรือแสดงบัญชีผู้ใช้พร้อมข้อมูลสิทธิ์
  - [`AccountController.Activity`](WebApplication1/Controllers/AccountController.cs#L130) `([FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — เปิดประวัติการทำรายการของแอดมิน
  - [`AccountController.DashboardAsync`](WebApplication1/Controllers/AccountController.cs#L134) `(string section, DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — แปลงตัวกรองเป็น DashboardRequest ส่งให้ Dashboard ตาม Role แล้วคืน View หรือ 404
  - [`AccountController.Profile`](WebApplication1/Controllers/AccountController.cs#L143) `(string tab = "details", CancellationToken cancellationToken = default) → Task<IActionResult>` — เปิดหรือประกอบข้อมูลโปรไฟล์ตามสิทธิ์ของบัญชีปัจจุบัน
  - [`AccountController.UpdateProfile`](WebApplication1/Controllers/AccountController.cs#L152) `(ProfileForm form, CancellationToken cancellationToken) → Task<IActionResult>` — ปรับข้อมูลส่วนตัวและอัปเดตข้อมูลที่ใช้แสดง
  - [`AccountController.UpdateAvatar`](WebApplication1/Controllers/AccountController.cs#L165) `(IFormFile? avatar, CancellationToken cancellationToken) → Task<IActionResult>` — เปลี่ยนรูปประจำตัวและอัปเดตข้อมูลรูปของบัญชี
  - [`AccountController.ChangePassword`](WebApplication1/Controllers/AccountController.cs#L179) `(PasswordChangeForm form, CancellationToken cancellationToken) → Task<IActionResult>` — ตรวจข้อมูลรหัสผ่านแล้วบันทึกรหัสใหม่เมื่อผ่านเงื่อนไข
  - [`AccountController.SaveAddress`](WebApplication1/Controllers/AccountController.cs#L192) `(AddressForm form, CancellationToken cancellationToken, bool checkout = false) → Task<IActionResult>` — รับฟอร์มและบันทึกที่อยู่ของผู้ใช้ปัจจุบัน
  - [`AccountController.DeleteAddress`](WebApplication1/Controllers/AccountController.cs#L204) `(int id, CancellationToken cancellationToken, bool checkout = false) → Task<IActionResult>` — ลบที่อยู่ที่เป็นของผู้ใช้ปัจจุบัน
  - [`AccountController.CheckoutAddresses`](WebApplication1/Controllers/AccountController.cs#L215) `(int userId, string? error, int? selectedId, CancellationToken cancellationToken) → Task<IActionResult>` — คืน HTML รายการที่อยู่ให้หน้าชำระเงินและส่งรหัสรายการที่เพิ่งบันทึก
  - [`AccountController.SetDefaultAddress`](WebApplication1/Controllers/AccountController.cs#L222) `(int id, CancellationToken cancellationToken) → Task<IActionResult>` — ตั้งที่อยู่หลักให้บัญชีของเจ้าของที่อยู่นั้น
  - [`AccountController.ProfileResult`](WebApplication1/Controllers/AccountController.cs#L231) `(string tab, bool succeeded, string? success, string? error) → IActionResult` — เตรียมข้อความผลบันทึกและกลับไปยังแท็บโปรไฟล์ที่เกี่ยวข้อง
  - [`AccountController.ProfileError`](WebApplication1/Controllers/AccountController.cs#L237) `(string tab) → IActionResult` — เตรียมข้อความผลบันทึกและกลับไปยังแท็บโปรไฟล์ที่เกี่ยวข้อง
  - [`AccountController.DashboardForCurrentUser`](WebApplication1/Controllers/AccountController.cs#L243) `() → IRoleDashboard` — เลือกบริการ Dashboard ตามบทบาทของผู้ใช้ปัจจุบัน
  - [`AccountController.BuildProfileAsync`](WebApplication1/Controllers/AccountController.cs#L247) `(string tab, CancellationToken cancellationToken) → Task<ProfilePageViewModel?>` — เปิดหรือประกอบข้อมูลโปรไฟล์ตามสิทธิ์ของบัญชีปัจจุบัน
- **[AccountingController.cs](WebApplication1/Controllers/AccountingController.cs)**
  - [`AccountingController.AccountingController`](WebApplication1/Controllers/AccountingController.cs#L15) `(IAccountingService accounting) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ AccountingController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`AccountingController.DeductCommission`](WebApplication1/Controllers/AccountingController.cs#L21) `(int? orderId, CancellationToken cancellationToken) → Task<IActionResult>` — ตรวจออเดอร์ที่ส่งถึงแล้วและบันทึกการหักค่าคอม
  - [`AccountingController.ApprovePayout`](WebApplication1/Controllers/AccountingController.cs#L36) `(int orderId, CancellationToken cancellationToken) → Task<IActionResult>` — อนุมัติยอดสุทธิของออเดอร์ที่ส่งถึงเมื่อผ่านเงื่อนไขการจ่าย
  - [`AccountingController.ApproveAfterCommission`](WebApplication1/Controllers/AccountingController.cs#L45) `(int orderId, [FromQuery] DashboardQuery query, CancellationToken cancellationToken) → Task<IActionResult>` — อนุมัติยอดสุทธิของออเดอร์ที่ส่งถึงเมื่อผ่านเงื่อนไขการจ่าย
  - [`AccountingController.RecordTransfer`](WebApplication1/Controllers/AccountingController.cs#L54) `(int orderId, DateOnly? transferDate, string? transferReference, IFormFile? slip, CancellationToken cancellationToken, [FromQuery] bool fromOrderStatus = false, [FromQuery] DashboardQuery? query = null) → Task<IActionResult>` — ตรวจยอดอนุมัติและบันทึกวันที่ เลขอ้างอิง และหลักฐานโอนให้ร้าน
  - [`AccountingController.ReturnToOrderStatus`](WebApplication1/Controllers/AccountingController.cs#L74) `(DashboardQuery query) → RedirectToActionResult` — กลับหน้าสถานะออเดอร์โดยเก็บค่าค้นหาและหน้าปัจจุบัน
  - [`AccountingController.AddEntry`](WebApplication1/Controllers/AccountingController.cs#L77) `(DateOnly entryDate, string entryType, string title, decimal amount, string? note, CancellationToken cancellationToken) → Task<IActionResult>` — ตรวจและบันทึกรายรับหรือรายจ่ายที่ฝ่ายบัญชีกรอกเอง
  - [`AccountingController.DeleteEntry`](WebApplication1/Controllers/AccountingController.cs#L86) `(int id, CancellationToken cancellationToken) → Task<IActionResult>` — นำรายการบัญชีที่บันทึกเองออกตามรหัสรายการ
  - [`AccountingController.ExportLedger`](WebApplication1/Controllers/AccountingController.cs#L95) `(DateOnly from, DateOnly to, CancellationToken cancellationToken) → Task<IActionResult>` — สร้างข้อมูลสมุดบัญชีในรูปแบบ CSV ตามช่วงวันที่
- **[AdminController.cs](WebApplication1/Controllers/AdminController.cs)**
  - [`AdminController.AdminController`](WebApplication1/Controllers/AdminController.cs#L22) `(AppDbContext context, IUserAdminService users, ICouponAdminService coupons) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ AdminController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`AdminController.SaveFlashSale`](WebApplication1/Controllers/AdminController.cs#L31) `([FromServices] FlashSaleAdmin flash, [FromForm] FlashSaleDraft draft, CancellationToken token) → Task<IActionResult>` — ตรวจข้อมูลแคมเปญ เวลา ราคา และโควตาก่อนสร้าง Flash Sale
  - [`AdminController.TryBangkokTime`](WebApplication1/Controllers/AdminController.cs#L52) `(string value, out DateTime utc) → bool` — อ่านวันเวลาในฟอร์มเป็นเวลาไทยและแปลงเป็น UTC
  - [`AdminController.SaveUser`](WebApplication1/Controllers/AdminController.cs#L61) `(UserForm form, CancellationToken cancellationToken) → Task<IActionResult>` — จัดการฟอร์มสร้างหรือแก้บัญชี และคืนผลการบันทึกในหลังบ้าน
  - [`AdminController.SetSuspended`](WebApplication1/Controllers/AdminController.cs#L70) `(int id, bool suspended, CancellationToken cancellationToken) → Task<IActionResult>` — เปลี่ยนสถานะระงับบัญชีตามสิทธิ์ผู้ดำเนินการ
  - [`AdminController.DeleteUser`](WebApplication1/Controllers/AdminController.cs#L78) `(int id, CancellationToken cancellationToken) → Task<IActionResult>` — ขอลบบัญชีเป้าหมายตามกฎการจัดการผู้ใช้
  - [`AdminController.SaveCoupon`](WebApplication1/Controllers/AdminController.cs#L86) `(CouponForm form, CancellationToken cancellationToken) → Task<IActionResult>` — จัดการข้อมูลหรือสถานะของคูปองจากหลังบ้าน
  - [`AdminController.ToggleCoupon`](WebApplication1/Controllers/AdminController.cs#L95) `(int id, CancellationToken cancellationToken) → Task<IActionResult>` — จัดการข้อมูลหรือสถานะของคูปองจากหลังบ้าน
  - [`AdminController.DeleteCoupon`](WebApplication1/Controllers/AdminController.cs#L103) `(int id, CancellationToken cancellationToken) → Task<IActionResult>` — จัดการข้อมูลหรือสถานะของคูปองจากหลังบ้าน
  - [`AdminController.Adduser`](WebApplication1/Controllers/AdminController.cs#L113) `(CancellationToken cancellationToken) → Task<IActionResult>` — จัดการฟอร์มสร้างหรือแก้บัญชี และคืนผลการบันทึกในหลังบ้าน
  - [`AdminController.Adduser`](WebApplication1/Controllers/AdminController.cs#L120) `(UserForm form, CancellationToken cancellationToken) → Task<IActionResult>` — จัดการฟอร์มสร้างหรือแก้บัญชี และคืนผลการบันทึกในหลังบ้าน
  - [`AdminController.userlist`](WebApplication1/Controllers/AdminController.cs#L139) `(CancellationToken cancellationToken) → Task<IActionResult>` — ค้นหรือแสดงบัญชีผู้ใช้พร้อมข้อมูลสิทธิ์
  - [`AdminController.UpdateRole`](WebApplication1/Controllers/AdminController.cs#L157) `(int id, string role, CancellationToken cancellationToken) → Task<IActionResult>` — เปลี่ยนบทบาทบัญชี และอัปเดตสิทธิ์ที่ระบบใช้
  - [`AdminController.RoleNamesAsync`](WebApplication1/Controllers/AdminController.cs#L175) `(CancellationToken cancellationToken) → Task<List<string>>` — อ่านรายชื่อบทบาทที่มีให้เลือกจากฐานข้อมูล
  - [`AdminController.FirstError`](WebApplication1/Controllers/AdminController.cs#L178) `() → string` — เลือกข้อความผิดพลาดตัวแรกจากผลตรวจข้อมูลฟอร์ม
  - [`AdminController.Back`](WebApplication1/Controllers/AdminController.cs#L183) `(string section, string? error, string? success = null) → IActionResult` — เตรียมข้อความผลการทำรายการและพากลับหน้าหลังบ้าน
- **[CheckoutController.cs](WebApplication1/Controllers/CheckoutController.cs)**
  - [`CheckoutController.CheckoutController`](WebApplication1/Controllers/CheckoutController.cs#L19) `(IPricingService pricing, ICheckoutService checkout, ICurrentUserService currentUser) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ CheckoutController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`CheckoutController.Quote`](WebApplication1/Controllers/CheckoutController.cs#L26) `([FromBody] QuoteRequest request, CancellationToken cancellationToken) → Task<IActionResult>` — คำนวณยอดตะกร้าจากราคา สต็อก ส่วนลด และค่าส่งที่เซิร์ฟเวอร์ตรวจได้
  - [`CheckoutController.PlaceOrder`](WebApplication1/Controllers/CheckoutController.cs#L51) `([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken) → Task<IActionResult>` — ตรวจคำขอสั่งซื้อ ยอดที่ยืนยัน และคำขอซ้ำก่อนบันทึกออเดอร์
- **[CouponsController.cs](WebApplication1/Controllers/CouponsController.cs)**
  - [`CouponsController.Save`](WebApplication1/Controllers/CouponsController.cs#L12) `(int couponId, CancellationToken token) → Task<IActionResult>` — เก็บคูปองเข้าบัญชีลูกค้า แล้วกลับไปหน้าโปรโมชั่นพร้อมแจ้งผล
- **[ErrorController.cs](WebApplication1/Controllers/ErrorController.cs)**
  - [`ErrorController.ErrorController`](WebApplication1/Controllers/ErrorController.cs#L16) `(ILogger<ErrorController> logger) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ErrorController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ErrorController.Status`](WebApplication1/Controllers/ErrorController.cs#L21) `(int statusCode) → IActionResult` — เลือกข้อความและ View แสดงข้อผิดพลาดตามรหัส HTTP
- **[HomeController.cs](WebApplication1/Controllers/HomeController.cs)**
  - [`HomeController.HomeController`](WebApplication1/Controllers/HomeController.cs#L26) `(AppDbContext context, ICurrentUserService currentUser, IAuthSession authSession, IProductCatalogService catalog, IReviewService reviews) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ HomeController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`HomeController.LoginDestination`](WebApplication1/Controllers/HomeController.cs#L36) `(string? role) → IActionResult` — เลือกหน้าแรกหรือหลังบ้านตามบทบาทหลังล็อกอิน
  - [`HomeController.Index`](WebApplication1/Controllers/HomeController.cs#L41) `(CancellationToken cancellationToken) → Task<IActionResult>` — รวบรวมข้อมูลสินค้า หมวด โปรโมชัน และ Flash Sale สำหรับหน้าแรก
  - [`HomeController.Product`](WebApplication1/Controllers/HomeController.cs#L58) `(string? category, string? search, ProductSort sort = ProductSort.Recommended, decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default) → Task<IActionResult>` — อ่านรายการสินค้าโดยใช้เงื่อนไขของหน้าปัจจุบัน
  - [`HomeController.Shop`](WebApplication1/Controllers/HomeController.cs#L63) `(int id, string? category, string? search, ProductSort sort = ProductSort.Recommended, decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default) → Task<IActionResult>` — อ่านข้อมูลร้านและเตรียมหน้าสินค้าเฉพาะร้าน
  - [`HomeController.BuildCatalogAsync`](WebApplication1/Controllers/HomeController.cs#L72) `(StorefrontShop? shop, string? category, string? search, ProductSort sort, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken) → Task<ProductListViewModel>` — ประกอบโมเดลหน้ารวมสินค้าจากตัวกรองและข้อมูลแค็ตตาล็อก
  - [`HomeController.ProductDetail`](WebApplication1/Controllers/HomeController.cs#L93) `(int id, CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมข้อมูลสินค้า รีวิว และรายการแนะนำบนหน้ารายละเอียด
  - [`HomeController.Promotion`](WebApplication1/Controllers/HomeController.cs#L120) `(CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมคูปองและสถานะเก็บหรือใช้แล้วสำหรับหน้าโปรโมชั่น
  - [`HomeController.Help`](WebApplication1/Controllers/HomeController.cs#L145) `() → IActionResult` — คืน View หน้าช่วยเหลือ
  - [`HomeController.Login`](WebApplication1/Controllers/HomeController.cs#L148) `(string? returnUrl = null) → IActionResult` — จัดการการเข้าสู่ระบบและข้อมูลที่เก็บในเซสชัน
  - [`HomeController.Login`](WebApplication1/Controllers/HomeController.cs#L158) `(LoginViewModel model) → Task<IActionResult>` — จัดการการเข้าสู่ระบบและข้อมูลที่เก็บในเซสชัน
  - [`HomeController.ForgotPassword`](WebApplication1/Controllers/HomeController.cs#L202) `() → IActionResult` — เปิดหรือประมวลผลฟอร์มกู้การเข้าถึงบัญชี
  - [`HomeController.ForgotPassword`](WebApplication1/Controllers/HomeController.cs#L212) `(ForgotPasswordViewModel model) → Task<IActionResult>` — เปิดหรือประมวลผลฟอร์มกู้การเข้าถึงบัญชี
  - [`HomeController.Register`](WebApplication1/Controllers/HomeController.cs#L239) `() → IActionResult` — เปิดหรือประมวลผลการสมัครบัญชีลูกค้า
  - [`HomeController.Register`](WebApplication1/Controllers/HomeController.cs#L249) `(RegisterViewModel model) → Task<IActionResult>` — เปิดหรือประมวลผลการสมัครบัญชีลูกค้า
  - [`HomeController.Logout`](WebApplication1/Controllers/HomeController.cs#L301) `() → IActionResult` — ล้างสถานะล็อกอินและจบเซสชันผู้ใช้
- **[MarketplaceController.cs](WebApplication1/Controllers/MarketplaceController.cs)**
  - [`MarketplaceController.SaveShop`](WebApplication1/Controllers/MarketplaceController.cs#L15) `(int id, string? name, string? pickupAddress, CancellationToken token) → Task<IActionResult>` — สร้างหรือแก้ชื่อร้านและจุดรับสินค้า
  - [`MarketplaceController.AssignShop`](WebApplication1/Controllers/MarketplaceController.cs#L40) `(int userId, int shopId, CancellationToken token) → Task<IActionResult>` — ผูกหรือค้นรหัสร้านที่บัญชี Shop มีสิทธิ์ดูแล
  - [`MarketplaceController.AssignProvince`](WebApplication1/Controllers/MarketplaceController.cs#L56) `(int userId, string? provinceCode, CancellationToken token) → Task<IActionResult>` — บันทึกจังหวัดที่ไรเดอร์รับผิดชอบ
  - [`MarketplaceController.HasRole`](WebApplication1/Controllers/MarketplaceController.cs#L71) `(int userId, string role, CancellationToken token) → Task<bool>` — ตรวจว่าบทบาทของบัญชีตรงกับเงื่อนไขที่กำหนด
  - [`MarketplaceController.Back`](WebApplication1/Controllers/MarketplaceController.cs#L75) `(string? error = null) → IActionResult` — เตรียมข้อความผลการทำรายการและพากลับหน้าหลังบ้าน
- **[OrdersController.cs](WebApplication1/Controllers/OrdersController.cs)**
  - [`OrdersController.OrdersController`](WebApplication1/Controllers/OrdersController.cs#L22) `(IOrderFulfillmentService fulfillment, AppDbContext db, ICurrentUserService currentUser) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ OrdersController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`OrdersController.Label`](WebApplication1/Controllers/OrdersController.cs#L30) `(string orderNo, CancellationToken cancellationToken) → Task<IActionResult>` — เตรียมหน้าป้ายจัดส่งของออเดอร์ที่ผู้ใช้มีสิทธิ์เปิด
  - [`OrdersController.MoveTo`](WebApplication1/Controllers/OrdersController.cs#L39) `(string orderNo, string status, string? returnSection, CancellationToken cancellationToken) → Task<IActionResult>` — ตรวจบทบาทและเปลี่ยนสถานะออเดอร์ตามเส้นทางที่อนุญาต
  - [`OrdersController.Cancel`](WebApplication1/Controllers/OrdersController.cs#L59) `(string orderNo, string? returnSection, CancellationToken cancellationToken) → Task<IActionResult>` — รับเลขออเดอร์และส่งต่อให้บริการตรวจสิทธิ์กับกฎการยกเลิก
- **[OwnerController.cs](WebApplication1/Controllers/OwnerController.cs)**
  - [`OwnerController.DemoteAdmin`](WebApplication1/Controllers/OwnerController.cs#L11) `(int id, CancellationToken cancellationToken) → Task<IActionResult>` — ตรวจสิทธิ์ Owner ก่อนลดบทบาทแอดมิน
- **[PaymentsController.cs](WebApplication1/Controllers/PaymentsController.cs)**
  - [`PaymentsController.Cancel`](WebApplication1/Controllers/PaymentsController.cs#L15) `([FromServices] BasketCancellationService cancellation, Guid id, string? reason, CancellationToken token) → Task<IActionResult>` — รับเหตุผลจากฝ่ายบัญชีและส่งรหัสยอดชำระรวมให้บริการยกเลิก
  - [`PaymentsController.BasketSlip`](WebApplication1/Controllers/PaymentsController.cs#L22) `(Guid id, CancellationToken token) → Task<IActionResult>` — อ่านและส่งรูปสลิปให้เฉพาะผู้มีสิทธิ์
  - [`PaymentsController.PayoutSlip`](WebApplication1/Controllers/PaymentsController.cs#L36) `(int id, CancellationToken token) → Task<IActionResult>` — อ่านและส่งรูปสลิปให้เฉพาะผู้มีสิทธิ์
  - [`PaymentsController.Review`](WebApplication1/Controllers/PaymentsController.cs#L47) `(Guid id, Guid slipId, bool approved, string? reason, CancellationToken token) → Task<IActionResult>` — ยืนยันหรือขอสลิปใหม่โดยตรวจว่ารหัสสลิปตรงกับใบที่กำลังรอตรวจ
  - [`PaymentsController.Resubmit`](WebApplication1/Controllers/PaymentsController.cs#L68) `(Guid id, IFormFile? slip, CancellationToken token) → Task<IActionResult>` — รับสลิปใหม่จากลูกค้าเจ้าของยอดที่ถูกขอหลักฐานเพิ่มเติม
  - [`PaymentsController.ProtectedImage`](WebApplication1/Controllers/PaymentsController.cs#L104) `(BankSlip? slip) → IActionResult` — อ่านและส่งรูปสลิปให้เฉพาะผู้มีสิทธิ์
- **[ReviewsController.cs](WebApplication1/Controllers/ReviewsController.cs)**
  - [`ReviewsController.ReviewsController`](WebApplication1/Controllers/ReviewsController.cs#L13) `(IReviewService reviews, ICurrentUserService currentUser) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ReviewsController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ReviewsController.Save`](WebApplication1/Controllers/ReviewsController.cs#L19) `(int productId, int rating, string? comment, CancellationToken cancellationToken) → Task<IActionResult>` — ตรวจและบันทึกข้อมูลที่คลาสนี้รับผิดชอบ
- **[ShipmentsController.cs](WebApplication1/Controllers/ShipmentsController.cs)**
  - [`ShipmentsController.ShipmentsController`](WebApplication1/Controllers/ShipmentsController.cs#L19) `(IShipmentService shipments) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ShipmentsController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ShipmentsController.Dispatch`](WebApplication1/Controllers/ShipmentsController.cs#L24) `(string orderNo, string? carrier, string? otherCarrierName, string? trackingNumber, CancellationToken cancellationToken) → Task<IActionResult>` — บันทึกผู้ให้บริการและเลขพัสดุ แล้วเปลี่ยนออเดอร์เป็นกำลังจัดส่ง
  - [`ShipmentsController.BackToPacking`](WebApplication1/Controllers/ShipmentsController.cs#L39) `(string? success = null, string? error = null) → RedirectToActionResult` — เตรียมข้อความผลการทำรายการและพากลับหน้าหลังบ้าน
- **[ShopContactController.cs](WebApplication1/Controllers/ShopContactController.cs)**
  - [`ShopContactController.Conversation`](WebApplication1/Controllers/ShopContactController.cs#L12) `(int id, CancellationToken token) → Task<IActionResult>` — เปิดและคัดข้อความในบทสนทนาตามลูกค้าและร้าน
  - [`ShopContactController.Send`](WebApplication1/Controllers/ShopContactController.cs#L19) `(int id, string? message, CancellationToken token) → Task<IActionResult>` — ตรวจสิทธิ์ในบทสนทนาแล้วบันทึกข้อความใหม่
  - [`ShopContactController.Follow`](WebApplication1/Controllers/ShopContactController.cs#L27) `(int id, bool follow, CancellationToken token) → Task<IActionResult>` — บันทึกการติดตามหรือเลิกติดตามร้านของลูกค้า
- **[ShopController.cs](WebApplication1/Controllers/ShopController.cs)**
  - [`ShopController.ShopController`](WebApplication1/Controllers/ShopController.cs#L19) `(IProductService products, ShopProfileService profile, ShopInteractions interactions, WebApplication1.Services.Identity.ICurrentUserService user) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ShopController และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ShopController.Reviews`](WebApplication1/Controllers/ShopController.cs#L28) `(CancellationToken token) → Task<IActionResult>` — ดึงรีวิวตามสินค้าหรือร้านที่อยู่ในขอบเขตของคำขอ
  - [`ShopController.Messages`](WebApplication1/Controllers/ShopController.cs#L32) `(CancellationToken token) → Task<IActionResult>` — อ่านกล่องข้อความของร้านและเตรียมหน้าแสดงบทสนทนา
  - [`ShopController.ReplyReview`](WebApplication1/Controllers/ShopController.cs#L36) `(int id, string? message, CancellationToken token) → Task<IActionResult>` — บันทึกคำตอบของร้านสำหรับรีวิวสินค้าที่ร้านมีสิทธิ์ดูแล
  - [`ShopController.SaveProfileImage`](WebApplication1/Controllers/ShopController.cs#L44) `(IFormFile? image, CancellationToken cancellationToken) → Task<IActionResult>` — บันทึกรูปโปรไฟล์ร้านและข้อมูลที่ใช้อ้างถึงรูป
  - [`ShopController.SaveProduct`](WebApplication1/Controllers/ShopController.cs#L52) `(ProductForm form, CancellationToken cancellationToken) → Task<IActionResult>` — จัดการข้อมูลหรือสต็อกสินค้าภายในร้านของบัญชีปัจจุบัน
  - [`ShopController.Restock`](WebApplication1/Controllers/ShopController.cs#L66) `(int id, int amount, CancellationToken cancellationToken) → Task<IActionResult>` — จัดการข้อมูลหรือสต็อกสินค้าภายในร้านของบัญชีปัจจุบัน
  - [`ShopController.DeleteProduct`](WebApplication1/Controllers/ShopController.cs#L74) `(int id, CancellationToken cancellationToken) → Task<IActionResult>` — จัดการข้อมูลหรือสต็อกสินค้าภายในร้านของบัญชีปัจจุบัน
  - [`ShopController.Back`](WebApplication1/Controllers/ShopController.cs#L82) `(string? error, string? success = null) → IActionResult` — เตรียมข้อความผลการทำรายการและพากลับหน้าหลังบ้าน

</details>

<details>
<summary><code>WebApplication1/Data</code></summary>

- **[AppDbContext.cs](WebApplication1/Data/AppDbContext.cs)**
  - [`AppDbContext.AppDbContext`](WebApplication1/Data/AppDbContext.cs#L13) `(DbContextOptions<AppDbContext> options) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ AppDbContext และรับค่าหรือบริการที่คลาสต้องใช้
  - [`AppDbContext.RecordAdminActivity`](WebApplication1/Data/AppDbContext.cs#L20) `(string actor, string description) → void` — เพิ่มข้อมูลการทำรายการของแอดมินลงประวัติ
  - [`AppDbContext.OnModelCreating`](WebApplication1/Data/AppDbContext.cs#L71) `(ModelBuilder modelBuilder) → void` — กำหนดแมปตาราง คีย์ และความสัมพันธ์ของโมเดล EF Core
- **[MarketplaceMapping.cs](WebApplication1/Data/MarketplaceMapping.cs)**
  - [`MarketplaceMapping.Configure`](WebApplication1/Data/MarketplaceMapping.cs#L8) `(ModelBuilder model) → void` — กำหนดแมปตาราง คีย์ และความสัมพันธ์ของโมเดล EF Core
- **[PayoutQueries.cs](WebApplication1/Data/PayoutQueries.cs)**
  - [`PayoutQueries.WherePayout`](WebApplication1/Data/PayoutQueries.cs#L10) `(this IQueryable<Order> orders, PayoutFilter filter) → IQueryable<Order>` — เติมเงื่อนไขตามตัวกรองการเงินและคืน IQueryable เพื่อใช้ประกอบ SQL ต่อ
- **[ShippingMapping.cs](WebApplication1/Data/ShippingMapping.cs)**
  - [`ShippingMapping.Configure`](WebApplication1/Data/ShippingMapping.cs#L11) `(ModelBuilder model) → void` — กำหนดแมปตาราง คีย์ และความสัมพันธ์ของโมเดล EF Core
- **[ShopScope.cs](WebApplication1/Data/ShopScope.cs)**
  - [`ShopScope.ShopIdAsync`](WebApplication1/Data/ShopScope.cs#L9) `(this AppDbContext db, int userId, CancellationToken token) → Task<int?>` — ค้นรหัสร้านที่ผูกกับบัญชีจากตารางสมาชิกของร้าน
  - [`ShopScope.ProductsFor`](WebApplication1/Data/ShopScope.cs#L12) `(this AppDbContext db, int userId) → IQueryable<Product>` — เติมเงื่อนไขให้อ่านเฉพาะสินค้าของร้านที่บัญชีมีสิทธิ์
  - [`ShopScope.OrdersFor`](WebApplication1/Data/ShopScope.cs#L16) `(this AppDbContext db, int userId) → IQueryable<Order>` — เติมเงื่อนไขให้อ่านเฉพาะออเดอร์ของร้านที่บัญชีมีสิทธิ์

</details>

<details>
<summary><code>WebApplication1/Data/Converters</code></summary>

- **[CarrierConverter.cs](WebApplication1/Data/Converters/CarrierConverter.cs)**
  - [`CarrierConverter.CarrierConverter`](WebApplication1/Data/Converters/CarrierConverter.cs#L9) `() → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ CarrierConverter และรับค่าหรือบริการที่คลาสต้องใช้
  - [`CarrierConverter.Parse`](WebApplication1/Data/Converters/CarrierConverter.cs#L14) `(string code) → Carrier` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
- **[DateOnlyConverter.cs](WebApplication1/Data/Converters/DateOnlyConverter.cs)**
  - [`DateOnlyConverter.DateOnlyConverter`](WebApplication1/Data/Converters/DateOnlyConverter.cs#L11) `() → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ DateOnlyConverter และรับค่าหรือบริการที่คลาสต้องใช้
- **[OrderStatusConverter.cs](WebApplication1/Data/Converters/OrderStatusConverter.cs)**
  - [`OrderStatusConverter.OrderStatusConverter`](WebApplication1/Data/Converters/OrderStatusConverter.cs#L12) `() → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ OrderStatusConverter และรับค่าหรือบริการที่คลาสต้องใช้

</details>

<details>
<summary><code>WebApplication1/Domain/Accounting</code></summary>

- **[PayoutStage.cs](WebApplication1/Domain/Accounting/PayoutStage.cs)**
  - [`PayoutStages.Resolve`](WebApplication1/Domain/Accounting/PayoutStage.cs#L26) `(OrderStatus delivery, bool approved, bool transferred) → PayoutStage` — จัดสถานะการเงินจากสถานะจัดส่ง การอนุมัติ และการมีวันที่โอน
  - [`PayoutStages.Label`](WebApplication1/Domain/Accounting/PayoutStage.cs#L35) `(this PayoutStage stage) → string` — เตรียมข้อความแสดงผลหรือหน้าป้ายสำหรับข้อมูลนั้น

</details>

<details>
<summary><code>WebApplication1/Domain/Addresses</code></summary>

- **[ThailandAddressCatalog.cs](WebApplication1/Domain/Addresses/ThailandAddressCatalog.cs)**
  - [`ThailandAddressCatalog.Format`](WebApplication1/Domain/Addresses/ThailandAddressCatalog.cs#L18) `(AddressRegion? region) → string?` — จัดรูปแบบค่าก่อนนำไปแสดงหรือส่งผ่านลิงก์

</details>

<details>
<summary><code>WebApplication1/Domain/Catalog</code></summary>

- **[StorefrontCategories.cs](WebApplication1/Domain/Catalog/StorefrontCategories.cs)**
  - [`StorefrontCategories.FindByKey`](WebApplication1/Domain/Catalog/StorefrontCategories.cs#L20) `(string? key) → StorefrontCategory?` — ค้นรายการจากคีย์หรือชื่อ
  - [`StorefrontCategories.FindByName`](WebApplication1/Domain/Catalog/StorefrontCategories.cs#L23) `(string? name) → StorefrontCategory?` — ค้นรายการจากคีย์หรือชื่อ

</details>

<details>
<summary><code>WebApplication1/Domain/Common</code></summary>

- **[OperationResult.cs](WebApplication1/Domain/Common/OperationResult.cs)**
  - [`OperationResult.OperationResult`](WebApplication1/Domain/Common/OperationResult.cs#L9) `(bool succeeded, T? value, string? error) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ OperationResult และรับค่าหรือบริการที่คลาสต้องใช้
  - [`OperationResult.Success`](WebApplication1/Domain/Common/OperationResult.cs#L22) `(T value) → OperationResult<T>` — สร้างผลลัพธ์สำเร็จพร้อมค่าที่ผู้เรียกอาจต้องใช้ต่อ
  - [`OperationResult.Failure`](WebApplication1/Domain/Common/OperationResult.cs#L24) `(string error) → OperationResult<T>` — สร้างผลลัพธ์ที่ระบุข้อผิดพลาดหรือเหตุผลปฏิเสธ
  - [`OperationResult.Ok`](WebApplication1/Domain/Common/OperationResult.cs#L29) `() → OperationResult<bool>` — สร้างผลลัพธ์สำเร็จพร้อมค่าที่ผู้เรียกอาจต้องใช้ต่อ
  - [`OperationResult.Fail`](WebApplication1/Domain/Common/OperationResult.cs#L31) `(string error) → OperationResult<bool>` — สร้างผลลัพธ์ที่ระบุข้อผิดพลาดหรือเหตุผลปฏิเสธ

</details>

<details>
<summary><code>WebApplication1/Domain/Identity</code></summary>

- **[RoleProfiles.cs](WebApplication1/Domain/Identity/RoleProfiles.cs)**
  - [`RoleProfiles.For`](WebApplication1/Domain/Identity/RoleProfiles.cs#L19) `(string? role) → RoleProfile` — เลือกข้อมูลประกอบจากค่าที่ส่งมา เช่น ข้อมูล Role หรือรายละเอียดการจัดส่ง
- **[Roles.cs](WebApplication1/Domain/Identity/Roles.cs)**
  - [`Roles.AreEqual`](WebApplication1/Domain/Identity/Roles.cs#L17) `(string? left, string? right) → bool` — ตรวจว่าบทบาทของบัญชีตรงกับเงื่อนไขที่กำหนด

</details>

<details>
<summary><code>WebApplication1/Domain/Orders</code></summary>

- **[DeliveryCoverage.cs](WebApplication1/Domain/Orders/DeliveryCoverage.cs)**
  - [`DeliveryCoverage.UsesRider`](WebApplication1/Domain/Orders/DeliveryCoverage.cs#L8) `(string? provinceCode) → bool` — ตรวจพื้นที่ไรเดอร์หรือแปลงรหัสจังหวัดให้เป็นชื่อที่แสดง
  - [`DeliveryCoverage.ProvinceName`](WebApplication1/Domain/Orders/DeliveryCoverage.cs#L10) `(string? provinceCode) → string?` — ตรวจพื้นที่ไรเดอร์หรือแปลงรหัสจังหวัดให้เป็นชื่อที่แสดง
- **[IOrderWorkflow.cs](WebApplication1/Domain/Orders/IOrderWorkflow.cs)**
  - [`IOrderWorkflow.CanTransition`](WebApplication1/Domain/Orders/IOrderWorkflow.cs#L8) `(OrderStatus from, OrderStatus to, string? actorRole) → bool` — ตรวจบทบาทและเปลี่ยนสถานะออเดอร์ตามเส้นทางที่อนุญาต
  - [`IOrderWorkflow.TrackingNoteFor`](WebApplication1/Domain/Orders/IOrderWorkflow.cs#L11) `(OrderStatus status) → string` — แปลงสถานะเป็นข้อความหรือลำดับขั้นสำหรับหน้าติดตาม
- **[OrderStatusCodes.cs](WebApplication1/Domain/Orders/OrderStatusCodes.cs)**
  - [`OrderStatusCodes.ToCode`](WebApplication1/Domain/Orders/OrderStatusCodes.cs#L22) `(this OrderStatus status) → string` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
  - [`OrderStatusCodes.TryParseCode`](WebApplication1/Domain/Orders/OrderStatusCodes.cs#L24) `(string? code, out OrderStatus status) → bool` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
  - [`OrderStatusCodes.ParseCode`](WebApplication1/Domain/Orders/OrderStatusCodes.cs#L27) `(string code) → OrderStatus` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
  - [`OrderStatusCodes.ToThai`](WebApplication1/Domain/Orders/OrderStatusCodes.cs#L32) `(this OrderStatus status) → string` — แปลงสถานะเป็นข้อความหรือลำดับขั้นสำหรับหน้าติดตาม
  - [`OrderStatusCodes.ToStepIndex`](WebApplication1/Domain/Orders/OrderStatusCodes.cs#L44) `(this OrderStatus status) → int` — แปลงสถานะเป็นข้อความหรือลำดับขั้นสำหรับหน้าติดตาม
- **[OrderWorkflow.cs](WebApplication1/Domain/Orders/OrderWorkflow.cs)**
  - [`OrderWorkflow.CanTransition`](WebApplication1/Domain/Orders/OrderWorkflow.cs#L24) `(OrderStatus from, OrderStatus to, string? actorRole) → bool` — ตรวจบทบาทและเปลี่ยนสถานะออเดอร์ตามเส้นทางที่อนุญาต
  - [`OrderWorkflow.TrackingNoteFor`](WebApplication1/Domain/Orders/OrderWorkflow.cs#L27) `(OrderStatus status) → string` — แปลงสถานะเป็นข้อความหรือลำดับขั้นสำหรับหน้าติดตาม
- **[PaymentMethods.cs](WebApplication1/Domain/Orders/PaymentMethods.cs)**
  - [`PaymentMethods.ToCode`](WebApplication1/Domain/Orders/PaymentMethods.cs#L23) `(this PaymentMethod method) → string` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
  - [`PaymentMethods.ToThai`](WebApplication1/Domain/Orders/PaymentMethods.cs#L25) `(this PaymentMethod method) → string` — แปลงสถานะเป็นข้อความหรือลำดับขั้นสำหรับหน้าติดตาม
  - [`PaymentMethods.TryParseCode`](WebApplication1/Domain/Orders/PaymentMethods.cs#L27) `(string? code, out PaymentMethod method) → bool` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
- **[PayoutRules.cs](WebApplication1/Domain/Orders/PayoutRules.cs)**
  - [`PayoutRules.CanApprove`](WebApplication1/Domain/Orders/PayoutRules.cs#L7) `(OrderStatus status, bool commissionDeducted, bool hasPayout) → bool` — ตรวจสถานะและข้อมูลว่าดำเนินการทางการเงินได้หรือไม่
  - [`PayoutRules.IsValidTransfer`](WebApplication1/Domain/Orders/PayoutRules.cs#L10) `(DateOnly? date, DateOnly today, string? reference) → bool` — ตรวจสถานะและข้อมูลว่าดำเนินการทางการเงินได้หรือไม่
  - [`PayoutRules.Commission`](WebApplication1/Domain/Orders/PayoutRules.cs#L14) `(decimal total, decimal rate) → decimal` — เปิดหรืออ่านรายการออเดอร์ส่งถึงแล้วที่รอหักค่าคอม
  - [`PayoutRules.StatusLabel`](WebApplication1/Domain/Orders/PayoutRules.cs#L16) `(bool commissionDeducted, bool approved, bool transferred) → string` — แปลงสถานะและข้อมูลประกอบเป็นสถานะที่หน้าเว็บนำไปแสดง

</details>

<details>
<summary><code>WebApplication1/Domain/Pricing</code></summary>

- **[CouponCalculator.cs](WebApplication1/Domain/Pricing/CouponCalculator.cs)**
  - [`CouponCalculator.Evaluate`](WebApplication1/Domain/Pricing/CouponCalculator.cs#L11) `(Coupon coupon, IReadOnlyList<PricedLine> lines, DateTime nowUtc) → CouponOutcome` — คำนวณส่วนลด แบ่งยอด หรือจำกัดจำนวนเงินตามกติกา
  - [`CouponCalculator.Cap`](WebApplication1/Domain/Pricing/CouponCalculator.cs#L48) `(decimal discount, decimal? maxDiscount, decimal eligibleSubtotal) → decimal` — คำนวณส่วนลด แบ่งยอด หรือจำกัดจำนวนเงินตามกติกา
- **[CouponTypes.cs](WebApplication1/Domain/Pricing/CouponTypes.cs)**
  - [`CouponTypes.ToThai`](WebApplication1/Domain/Pricing/CouponTypes.cs#L12) `(string type) → string` — แปลงสถานะเป็นข้อความหรือลำดับขั้นสำหรับหน้าติดตาม
- **[LoyaltyRules.cs](WebApplication1/Domain/Pricing/LoyaltyRules.cs)**
  - [`LoyaltyRules.Balance`](WebApplication1/Domain/Pricing/LoyaltyRules.cs#L12) `(decimal eligibleSpend, decimal used) → LoyaltyBalance` — คำนวณยอดเงินสะสมที่ใช้ได้และยอดคืบหน้าไปยังบาทถัดไป
  - [`LoyaltyRules.Discount`](WebApplication1/Domain/Pricing/LoyaltyRules.cs#L15) `(decimal requested, decimal available, decimal subtotal, decimal couponDiscount) → decimal` — คำนวณส่วนลด แบ่งยอด หรือจำกัดจำนวนเงินตามกติกา
  - [`LoyaltyRules.Allocate`](WebApplication1/Domain/Pricing/LoyaltyRules.cs#L18) `(decimal discount, decimal portion, decimal remainingSubtotal) → decimal` — แบ่งยอดส่วนลดตามสัดส่วนยอดสินค้าและปัดเป็นทศนิยมสองตำแหน่ง
- **[PricingModels.cs](WebApplication1/Domain/Pricing/PricingModels.cs)**
  - [`CouponOutcome.Rejected`](WebApplication1/Domain/Pricing/PricingModels.cs#L39) `(string error) → CouponOutcome` — สร้างผลลัพธ์ที่ระบุข้อผิดพลาดหรือเหตุผลปฏิเสธ

</details>

<details>
<summary><code>WebApplication1/Domain/Shipping</code></summary>

- **[Carrier.cs](WebApplication1/Domain/Shipping/Carrier.cs)**
  - [`Carriers.ToCode`](WebApplication1/Domain/Shipping/Carrier.cs#L38) `(this Carrier carrier) → string` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
  - [`Carriers.ToThai`](WebApplication1/Domain/Shipping/Carrier.cs#L40) `(this Carrier carrier) → string` — แปลงสถานะเป็นข้อความหรือลำดับขั้นสำหรับหน้าติดตาม
  - [`Carriers.TryParseCode`](WebApplication1/Domain/Shipping/Carrier.cs#L43) `(string? code, out Carrier carrier) → bool` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
- **[ShipmentDetails.cs](WebApplication1/Domain/Shipping/ShipmentDetails.cs)**
  - [`ShipmentDetails.ShipmentDetails`](WebApplication1/Domain/Shipping/ShipmentDetails.cs#L18) `(Carrier carrier, string? otherCarrierName, string trackingNumber) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ShipmentDetails และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ShipmentDetails.Create`](WebApplication1/Domain/Shipping/ShipmentDetails.cs#L40) `(string? carrierCode, string? otherCarrierName, string? trackingNumber) → OperationResult<ShipmentDetails>` — ตรวจชื่อผู้ให้บริการและเลขติดตาม แล้วคืนรายละเอียดพัสดุที่ผ่านการตรวจ
  - [`ShipmentDetails.FromStored`](WebApplication1/Domain/Shipping/ShipmentDetails.cs#L64) `(Carrier carrier, string? otherCarrierName, string trackingNumber) → ShipmentDetails` — แปลงระหว่างรหัสที่เก็บหรือรับจากฟอร์มกับค่าที่โค้ดใช้
  - [`ShipmentDetails.Separators`](WebApplication1/Domain/Shipping/ShipmentDetails.cs#L67) `() → Regex` — ตรวจอักขระคั่นในข้อมูลที่จะส่งออก
  - [`ShipmentDetails.TrackingPattern`](WebApplication1/Domain/Shipping/ShipmentDetails.cs#L71) `() → Regex` — ตรวจความซ้ำหรือรูปแบบของเลขติดตามพัสดุ

</details>

<details>
<summary><code>WebApplication1/Extensions</code></summary>

- **[ServiceCollectionExtensions.cs](WebApplication1/Extensions/ServiceCollectionExtensions.cs)**
  - [`ServiceCollectionExtensions.AddLoveCatApplication`](WebApplication1/Extensions/ServiceCollectionExtensions.cs#L18) `(this IServiceCollection services, IConfiguration configuration) → IServiceCollection` — ลงทะเบียนบริการของ LoveCat ให้ Dependency Injection สร้างและส่งต่อได้

</details>

<details>
<summary><code>WebApplication1/Filters</code></summary>

- **[AuthorizationResults.cs](WebApplication1/Filters/AuthorizationResults.cs)**
  - [`AuthorizationResults.NotSignedIn`](WebApplication1/Filters/AuthorizationResults.cs#L13) `(AuthorizationFilterContext context) → IActionResult` — ตรวจสิทธิ์และเลือกผลตอบกลับสำหรับหน้าเว็บหรือคำขอ API
  - [`AuthorizationResults.Forbidden`](WebApplication1/Filters/AuthorizationResults.cs#L26) `(AuthorizationFilterContext context) → IActionResult` — ตรวจสิทธิ์และเลือกผลตอบกลับสำหรับหน้าเว็บหรือคำขอ API
  - [`AuthorizationResults.IsApiRequest`](WebApplication1/Filters/AuthorizationResults.cs#L44) `(HttpRequest request) → bool` — ตรวจสิทธิ์และเลือกผลตอบกลับสำหรับหน้าเว็บหรือคำขอ API
- **[RequireLoginAttribute.cs](WebApplication1/Filters/RequireLoginAttribute.cs)**
  - [`RequireLoginAttribute.OnAuthorization`](WebApplication1/Filters/RequireLoginAttribute.cs#L13) `(AuthorizationFilterContext context) → void` — ตรวจเซสชันและสิทธิ์ก่อนอนุญาตให้ Action ทำงาน
  - [`RequireLoginAttribute.IsAllowed`](WebApplication1/Filters/RequireLoginAttribute.cs#L33) `(ICurrentUserService currentUser) → bool` — ตรวจสิทธิ์และเลือกผลตอบกลับสำหรับหน้าเว็บหรือคำขอ API
- **[RequireRoleAttribute.cs](WebApplication1/Filters/RequireRoleAttribute.cs)**
  - [`RequireRoleAttribute.RequireRoleAttribute`](WebApplication1/Filters/RequireRoleAttribute.cs#L11) `(params string[] allowedRoles) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ RequireRoleAttribute และรับค่าหรือบริการที่คลาสต้องใช้
  - [`RequireRoleAttribute.IsAllowed`](WebApplication1/Filters/RequireRoleAttribute.cs#L23) `(ICurrentUserService currentUser) → bool` — ตรวจว่าบัญชีปัจจุบันมีหนึ่งในบทบาทที่กำหนดไว้หรือไม่

</details>

<details>
<summary><code>WebApplication1/Helpers</code></summary>

- **[PasswordHelper.cs](WebApplication1/Helpers/PasswordHelper.cs)**
  - [`PasswordHelper.HashPassword`](WebApplication1/Helpers/PasswordHelper.cs#L18) `(string password) → string` — แปลงรหัสผ่านเป็น hash หรือเปรียบเทียบกับค่าที่จัดเก็บ
  - [`PasswordHelper.VerifyPassword`](WebApplication1/Helpers/PasswordHelper.cs#L29) `(string password, string storedHash) → bool` — แปลงรหัสผ่านเป็น hash หรือเปรียบเทียบกับค่าที่จัดเก็บ

</details>

<details>
<summary><code>WebApplication1/Middleware</code></summary>

- **[MembershipRefreshMiddleware.cs](WebApplication1/Middleware/MembershipRefreshMiddleware.cs)**
  - [`MembershipRefreshMiddleware.MembershipRefreshMiddleware`](WebApplication1/Middleware/MembershipRefreshMiddleware.cs#L16) `(RequestDelegate next) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ MembershipRefreshMiddleware และรับค่าหรือบริการที่คลาสต้องใช้
  - [`MembershipRefreshMiddleware.InvokeAsync`](WebApplication1/Middleware/MembershipRefreshMiddleware.cs#L21) `(HttpContext context, ICurrentUserService currentUser, IAuthSession authSession, AppDbContext db) → Task` — เตรียมข้อมูลให้ส่วนที่ใช้ซ้ำหรือทำงานใน middleware ตามชนิดคลาส
  - [`MembershipRefreshMiddleware.IsStaticFile`](WebApplication1/Middleware/MembershipRefreshMiddleware.cs#L52) `(PathString path) → bool` — ตรวจว่าคำขอเป็นไฟล์ที่เสิร์ฟตรงจากเว็บหรือไม่

</details>

<details>
<summary><code>WebApplication1/Models</code></summary>

- **[SavedAddressRegion.cs](WebApplication1/Models/SavedAddressRegion.cs)**
  - [`SavedAddressRegion.CopyFrom`](WebApplication1/Models/SavedAddressRegion.cs#L17) `(SavedAddressRegion region) → void` — แปลงหรือคัดลอกข้อมูลจากชนิดต้นทางไปยังชนิดที่ส่วนถัดไปใช้

</details>

<details>
<summary><code>WebApplication1/Services/Accounts</code></summary>

- **[AddressService.cs](WebApplication1/Services/Accounts/AddressService.cs)**
  - [`AddressService.AddressService`](WebApplication1/Services/Accounts/AddressService.cs#L16) `(AppDbContext db) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ AddressService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`AddressService.GetForUserAsync`](WebApplication1/Services/Accounts/AddressService.cs#L21) `(int userId, CancellationToken cancellationToken = default) → Task<IReadOnlyList<UserAddress>>` — อ่านข้อมูลที่ผูกกับผู้ใช้ตามรหัสบัญชี
  - [`AddressService.SaveAsync`](WebApplication1/Services/Accounts/AddressService.cs#L27) `(int userId, UserAddress input, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — ตรวจที่อยู่และสิทธิ์เจ้าของแล้วบันทึก พร้อมคืนรหัสที่อยู่
  - [`AddressService.DeleteAsync`](WebApplication1/Services/Accounts/AddressService.cs#L69) `(int userId, int addressId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข
  - [`AddressService.SetDefaultAsync`](WebApplication1/Services/Accounts/AddressService.cs#L91) `(int userId, int addressId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตั้งที่อยู่หลักให้บัญชีของเจ้าของที่อยู่นั้น
  - [`AddressService.Validate`](WebApplication1/Services/Accounts/AddressService.cs#L108) `(UserAddress input) → string?` — ตรวจข้อมูลนำเข้าและคืนเหตุผลเมื่อพบข้อมูลผิดเงื่อนไข
  - [`AddressService.ThaiMobile`](WebApplication1/Services/Accounts/AddressService.cs#L117) `() → Regex` — คืนรูปแบบตรวจหมายเลขโทรศัพท์มือถือไทย
- **[CouponCollection.cs](WebApplication1/Services/Accounts/CouponCollection.cs)**
  - [`CouponCollection.SaveAsync`](WebApplication1/Services/Accounts/CouponCollection.cs#L10) `(int userId, int couponId, CancellationToken token = default) → Task<OperationResult<bool>>` — ตรวจว่าคูปองเก็บได้และบันทึกให้ลูกค้า โดยใช้รายการเดิมหากเคยเก็บแล้ว
- **[IAddressService.cs](WebApplication1/Services/Accounts/IAddressService.cs)**
  - [`IAddressService.GetForUserAsync`](WebApplication1/Services/Accounts/IAddressService.cs#L9) `(int userId, CancellationToken cancellationToken = default) → Task<IReadOnlyList<UserAddress>>` — อ่านข้อมูลที่ผูกกับผู้ใช้ตามรหัสบัญชี
  - [`IAddressService.SaveAsync`](WebApplication1/Services/Accounts/IAddressService.cs#L12) `(int userId, UserAddress address, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — ตรวจและบันทึกข้อมูลที่คลาสนี้รับผิดชอบ
  - [`IAddressService.DeleteAsync`](WebApplication1/Services/Accounts/IAddressService.cs#L14) `(int userId, int addressId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข
  - [`IAddressService.SetDefaultAsync`](WebApplication1/Services/Accounts/IAddressService.cs#L16) `(int userId, int addressId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตั้งที่อยู่หลักให้บัญชีของเจ้าของที่อยู่นั้น
- **[IProfileService.cs](WebApplication1/Services/Accounts/IProfileService.cs)**
  - [`IProfileService.UpdateDetailsAsync`](WebApplication1/Services/Accounts/IProfileService.cs#L9) `(int userId, ProfileForm form, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ปรับข้อมูลส่วนตัวและอัปเดตข้อมูลที่ใช้แสดง
  - [`IProfileService.UpdateAvatarAsync`](WebApplication1/Services/Accounts/IProfileService.cs#L11) `(int userId, IFormFile file, CancellationToken cancellationToken = default) → Task<OperationResult<string>>` — เปลี่ยนรูปประจำตัวและอัปเดตข้อมูลรูปของบัญชี
  - [`IProfileService.ChangePasswordAsync`](WebApplication1/Services/Accounts/IProfileService.cs#L13) `(int userId, PasswordChangeForm form, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจข้อมูลรหัสผ่านแล้วบันทึกรหัสใหม่เมื่อผ่านเงื่อนไข
- **[ProfileService.cs](WebApplication1/Services/Accounts/ProfileService.cs)**
  - [`ProfileService.ProfileService`](WebApplication1/Services/Accounts/ProfileService.cs#L17) `(AppDbContext db, IImageStorage images, IAuthSession authSession) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ProfileService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ProfileService.UpdateDetailsAsync`](WebApplication1/Services/Accounts/ProfileService.cs#L24) `(int userId, ProfileForm form, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ปรับข้อมูลส่วนตัวและอัปเดตข้อมูลที่ใช้แสดง
  - [`ProfileService.UpdateAvatarAsync`](WebApplication1/Services/Accounts/ProfileService.cs#L46) `(int userId, IFormFile file, CancellationToken cancellationToken = default) → Task<OperationResult<string>>` — เปลี่ยนรูปประจำตัวและอัปเดตข้อมูลรูปของบัญชี
  - [`ProfileService.ChangePasswordAsync`](WebApplication1/Services/Accounts/ProfileService.cs#L62) `(int userId, PasswordChangeForm form, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจข้อมูลรหัสผ่านแล้วบันทึกรหัสใหม่เมื่อผ่านเงื่อนไข

</details>

<details>
<summary><code>WebApplication1/Services/Backoffice</code></summary>

- **[AccountingService.cs](WebApplication1/Services/Backoffice/AccountingService.cs)**
  - [`AccountingService.AccountingService`](WebApplication1/Services/Backoffice/AccountingService.cs#L21) `(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ AccountingService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`AccountingService.StartOfDayUtc`](WebApplication1/Services/Backoffice/AccountingService.cs#L36) `(DateOnly day) → DateTime` — เตรียมช่วงวันและข้อมูลสรุปตามวันที่ของระบบ
  - [`AccountingService.GetLedgerAsync`](WebApplication1/Services/Backoffice/AccountingService.cs#L38) `(DateOnly from, DateOnly to, CancellationToken cancellationToken = default) → Task<LedgerReport>` — เปิดหรือรวบรวมรายการบัญชีในช่วงวันที่ที่เลือก
  - [`AccountingService.Expense`](WebApplication1/Services/Backoffice/AccountingService.cs#L117) `(LedgerLineKind kind) → decimal` — รวมยอดรายจ่ายเฉพาะประเภทที่กำหนด
  - [`AccountingService.GetAwaitingCommissionAsync`](WebApplication1/Services/Backoffice/AccountingService.cs#L132) `(CancellationToken cancellationToken = default) → Task<IReadOnlyList<CommissionRow>>` — เปิดหรืออ่านรายการออเดอร์ส่งถึงแล้วที่รอหักค่าคอม
  - [`AccountingService.DeductCommissionAsync`](WebApplication1/Services/Backoffice/AccountingService.cs#L147) `(int? orderId, CancellationToken cancellationToken = default) → Task<OperationResult<decimal>>` — ตรวจออเดอร์ที่ส่งถึงแล้วและบันทึกการหักค่าคอม
  - [`AccountingService.GetPayoutQueueAsync`](WebApplication1/Services/Backoffice/AccountingService.cs#L176) `(CancellationToken cancellationToken) → Task<IReadOnlyList<PayoutQueueRow>>` — อ่านคิวออเดอร์ที่อยู่ระหว่างจัดการยอดสุทธิให้ร้าน
  - [`AccountingService.ApprovePayoutAsync`](WebApplication1/Services/Backoffice/AccountingService.cs#L189) `(int orderId, CancellationToken cancellationToken, bool deductCommission = false) → Task<OperationResult<bool>>` — อนุมัติยอดสุทธิของออเดอร์ที่ส่งถึงเมื่อผ่านเงื่อนไขการจ่าย
  - [`AccountingService.RecordTransferAsync`](WebApplication1/Services/Backoffice/AccountingService.cs#L234) `(int orderId, DateOnly? date, string? reference, CancellationToken cancellationToken, BankSlip? slip = null) → Task<OperationResult<bool>>` — ตรวจยอดอนุมัติและบันทึกวันที่ เลขอ้างอิง และหลักฐานโอนให้ร้าน
  - [`AccountingService.AddEntryAsync`](WebApplication1/Services/Backoffice/AccountingService.cs#L268) `(DateOnly date, string entryType, string title, decimal amount, string? note, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจและบันทึกรายรับหรือรายจ่ายที่ฝ่ายบัญชีกรอกเอง
  - [`AccountingService.DeleteEntryAsync`](WebApplication1/Services/Backoffice/AccountingService.cs#L289) `(int entryId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — นำรายการบัญชีที่บันทึกเองออกตามรหัสรายการ
  - [`AccountingService.ToCsv`](WebApplication1/Services/Backoffice/AccountingService.cs#L295) `(LedgerReport report) → byte[]` — สร้างข้อมูลสมุดบัญชีในรูปแบบ CSV ตามช่วงวันที่
  - [`AccountingService.Escape`](WebApplication1/Services/Backoffice/AccountingService.cs#L312) `(string? value) → string` — ปรับข้อความก่อนแสดงหรือส่งออก เช่น ปิดบางส่วนของชื่อหรือตัดข้อความยาว
- **[CouponAdminService.cs](WebApplication1/Services/Backoffice/CouponAdminService.cs)**
  - [`CouponAdminService.CouponAdminService`](WebApplication1/Services/Backoffice/CouponAdminService.cs#L17) `(AppDbContext db, ICurrentUserService currentUser) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ CouponAdminService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`CouponAdminService.SaveAsync`](WebApplication1/Services/Backoffice/CouponAdminService.cs#L25) `(CouponForm form, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — ตรวจรหัส ประเภท มูลค่า และเงื่อนไขส่วนลดก่อนบันทึกคูปอง
  - [`CouponAdminService.ToggleAsync`](WebApplication1/Services/Backoffice/CouponAdminService.cs#L82) `(int couponId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — สลับสถานะเปิดหรือปิดของรายการ
  - [`CouponAdminService.DeleteAsync`](WebApplication1/Services/Backoffice/CouponAdminService.cs#L96) `(int couponId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข
- **[FlashSaleAdmin.cs](WebApplication1/Services/Backoffice/FlashSaleAdmin.cs)**
  - [`FlashSaleAdmin.CreateAsync`](WebApplication1/Services/Backoffice/FlashSaleAdmin.cs#L15) `(int productId, decimal price, DateTime start, DateTime end, int quota, CancellationToken token) → Task<OperationResult<bool>>` — ตรวจข้อมูลแคมเปญ เวลา ราคา และโควตาก่อนสร้าง Flash Sale
- **[IAccountingService.cs](WebApplication1/Services/Backoffice/IAccountingService.cs)**
  - [`IAccountingService.GetLedgerAsync`](WebApplication1/Services/Backoffice/IAccountingService.cs#L10) `(DateOnly from, DateOnly to, CancellationToken cancellationToken = default) → Task<LedgerReport>` — เปิดหรือรวบรวมรายการบัญชีในช่วงวันที่ที่เลือก
  - [`IAccountingService.GetAwaitingCommissionAsync`](WebApplication1/Services/Backoffice/IAccountingService.cs#L12) `(CancellationToken cancellationToken = default) → Task<IReadOnlyList<CommissionRow>>` — เปิดหรืออ่านรายการออเดอร์ส่งถึงแล้วที่รอหักค่าคอม
  - [`IAccountingService.DeductCommissionAsync`](WebApplication1/Services/Backoffice/IAccountingService.cs#L14) `(int? orderId, CancellationToken cancellationToken = default) → Task<OperationResult<decimal>>` — ตรวจออเดอร์ที่ส่งถึงแล้วและบันทึกการหักค่าคอม
  - [`IAccountingService.GetPayoutQueueAsync`](WebApplication1/Services/Backoffice/IAccountingService.cs#L16) `(CancellationToken cancellationToken) → Task<IReadOnlyList<PayoutQueueRow>>` — อ่านคิวออเดอร์ที่อยู่ระหว่างจัดการยอดสุทธิให้ร้าน
  - [`IAccountingService.ApprovePayoutAsync`](WebApplication1/Services/Backoffice/IAccountingService.cs#L17) `(int orderId, CancellationToken cancellationToken, bool deductCommission = false) → Task<OperationResult<bool>>` — อนุมัติยอดสุทธิของออเดอร์ที่ส่งถึงเมื่อผ่านเงื่อนไขการจ่าย
  - [`IAccountingService.RecordTransferAsync`](WebApplication1/Services/Backoffice/IAccountingService.cs#L18) `(int orderId, DateOnly? date, string? reference, CancellationToken cancellationToken, WebApplication1.Models.BankSlip? slip = null) → Task<OperationResult<bool>>` — ตรวจยอดอนุมัติและบันทึกวันที่ เลขอ้างอิง และหลักฐานโอนให้ร้าน
  - [`IAccountingService.AddEntryAsync`](WebApplication1/Services/Backoffice/IAccountingService.cs#L20) `(DateOnly date, string entryType, string title, decimal amount, string? note, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจและบันทึกรายรับหรือรายจ่ายที่ฝ่ายบัญชีกรอกเอง
  - [`IAccountingService.DeleteEntryAsync`](WebApplication1/Services/Backoffice/IAccountingService.cs#L22) `(int entryId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — นำรายการบัญชีที่บันทึกเองออกตามรหัสรายการ
  - [`IAccountingService.ToCsv`](WebApplication1/Services/Backoffice/IAccountingService.cs#L25) `(LedgerReport report) → byte[]` — สร้างข้อมูลสมุดบัญชีในรูปแบบ CSV ตามช่วงวันที่
- **[ICouponAdminService.cs](WebApplication1/Services/Backoffice/ICouponAdminService.cs)**
  - [`ICouponAdminService.SaveAsync`](WebApplication1/Services/Backoffice/ICouponAdminService.cs#L8) `(CouponForm form, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — ตรวจและบันทึกข้อมูลที่คลาสนี้รับผิดชอบ
  - [`ICouponAdminService.ToggleAsync`](WebApplication1/Services/Backoffice/ICouponAdminService.cs#L10) `(int couponId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — สลับสถานะเปิดหรือปิดของรายการ
  - [`ICouponAdminService.DeleteAsync`](WebApplication1/Services/Backoffice/ICouponAdminService.cs#L12) `(int couponId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข
- **[IProductService.cs](WebApplication1/Services/Backoffice/IProductService.cs)**
  - [`IProductService.SaveAsync`](WebApplication1/Services/Backoffice/IProductService.cs#L9) `(ProductForm form, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — ตรวจและบันทึกข้อมูลที่คลาสนี้รับผิดชอบ
  - [`IProductService.RestockAsync`](WebApplication1/Services/Backoffice/IProductService.cs#L11) `(int productId, int amount, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — จัดการข้อมูลหรือสต็อกสินค้าภายในร้านของบัญชีปัจจุบัน
  - [`IProductService.DeleteAsync`](WebApplication1/Services/Backoffice/IProductService.cs#L13) `(int productId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข
- **[IUserAdminService.cs](WebApplication1/Services/Backoffice/IUserAdminService.cs)**
  - [`IUserAdminService.SaveAsync`](WebApplication1/Services/Backoffice/IUserAdminService.cs#L9) `(UserForm form, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — ตรวจและบันทึกข้อมูลที่คลาสนี้รับผิดชอบ
  - [`IUserAdminService.DemoteAdminAsync`](WebApplication1/Services/Backoffice/IUserAdminService.cs#L11) `(int userId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจสิทธิ์ Owner ก่อนลดบทบาทแอดมิน
  - [`IUserAdminService.SetSuspendedAsync`](WebApplication1/Services/Backoffice/IUserAdminService.cs#L13) `(int userId, bool suspended, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — เปลี่ยนสถานะระงับบัญชีตามสิทธิ์ผู้ดำเนินการ
  - [`IUserAdminService.DeleteAsync`](WebApplication1/Services/Backoffice/IUserAdminService.cs#L15) `(int userId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข
- **[PayoutSummaryReader.cs](WebApplication1/Services/Backoffice/PayoutSummaryReader.cs)**
  - [`PayoutSummaryReader.ReadAsync`](WebApplication1/Services/Backoffice/PayoutSummaryReader.cs#L15) `(IQueryable<Order> orders, CancellationToken token) → Task<PayoutSummary>` — นับจำนวนในแต่ละสถานะและรวมยอดสุทธิของออเดอร์พร้อมโอน
- **[ProductService.cs](WebApplication1/Services/Backoffice/ProductService.cs)**
  - [`ProductService.ProductService`](WebApplication1/Services/Backoffice/ProductService.cs#L21) `(AppDbContext db, IImageStorage images, TimeProvider clock, ICurrentUserService actor) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ProductService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ProductService.SaveAsync`](WebApplication1/Services/Backoffice/ProductService.cs#L29) `(ProductForm form, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — ตรวจสินค้าและร้านที่บัญชีดูแลก่อนเพิ่มหรือแก้สินค้า
  - [`ProductService.RestockAsync`](WebApplication1/Services/Backoffice/ProductService.cs#L91) `(int productId, int amount, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — จัดการข้อมูลหรือสต็อกสินค้าภายในร้านของบัญชีปัจจุบัน
  - [`ProductService.DeleteAsync`](WebApplication1/Services/Backoffice/ProductService.cs#L110) `(int productId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข
- **[ShopInteractions.cs](WebApplication1/Services/Backoffice/ShopInteractions.cs)**
  - [`ShopInteractions.SellerReviews`](WebApplication1/Services/Backoffice/ShopInteractions.cs#L13) `(int sellerId) → IQueryable<ProductReview>` — ดึงรีวิวตามสินค้าหรือร้านที่อยู่ในขอบเขตของคำขอ
  - [`ShopInteractions.ReviewsAsync`](WebApplication1/Services/Backoffice/ShopInteractions.cs#L16) `(int sellerId, CancellationToken token) → Task<IReadOnlyList<SellerReview>>` — ดึงรีวิวตามสินค้าหรือร้านที่อยู่ในขอบเขตของคำขอ
  - [`ShopInteractions.ReplyAsync`](WebApplication1/Services/Backoffice/ShopInteractions.cs#L24) `(int sellerId, int reviewId, string? message, CancellationToken token) → Task<OperationResult<bool>>` — บันทึกคำตอบของร้านสำหรับรีวิวสินค้าที่ร้านมีสิทธิ์ดูแล
  - [`ShopInteractions.SetFollowAsync`](WebApplication1/Services/Backoffice/ShopInteractions.cs#L52) `(int userId, int shopId, bool follow, CancellationToken token) → Task` — บันทึกการติดตามหรือเลิกติดตามร้านของลูกค้า
  - [`ShopInteractions.InboxAsync`](WebApplication1/Services/Backoffice/ShopInteractions.cs#L73) `(int sellerId, CancellationToken token) → Task<IReadOnlyList<int>>` — อ่านกล่องข้อความของร้านและเตรียมหน้าแสดงบทสนทนา
  - [`ShopInteractions.Thread`](WebApplication1/Services/Backoffice/ShopInteractions.cs#L81) `(int shopId, int customerId) → IQueryable<ShopMessage>` — เปิดและคัดข้อความในบทสนทนาตามลูกค้าและร้าน
  - [`ShopInteractions.ConversationAsync`](WebApplication1/Services/Backoffice/ShopInteractions.cs#L84) `(int actorId, bool seller, int id, CancellationToken token) → Task<ShopConversation?>` — เปิดและคัดข้อความในบทสนทนาตามลูกค้าและร้าน
  - [`ShopInteractions.SendAsync`](WebApplication1/Services/Backoffice/ShopInteractions.cs#L97) `(int actorId, bool seller, int id, string? message, CancellationToken token) → Task<OperationResult<bool>>` — ตรวจสิทธิ์ในบทสนทนาแล้วบันทึกข้อความใหม่
- **[ShopProfileService.cs](WebApplication1/Services/Backoffice/ShopProfileService.cs)**
  - [`ShopProfileService.SaveImageAsync`](WebApplication1/Services/Backoffice/ShopProfileService.cs#L12) `(IFormFile? file, CancellationToken token) → Task<OperationResult<bool>>` — บันทึกรูปโปรไฟล์ร้านและข้อมูลที่ใช้อ้างถึงรูป
- **[UserAdminService.cs](WebApplication1/Services/Backoffice/UserAdminService.cs)**
  - [`UserAdminService.UserAdminService`](WebApplication1/Services/Backoffice/UserAdminService.cs#L18) `(AppDbContext db, ICurrentUserService currentUser, IAuthSession authSession) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ UserAdminService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`UserAdminService.SaveAsync`](WebApplication1/Services/Backoffice/UserAdminService.cs#L27) `(UserForm form, CancellationToken cancellationToken = default) → Task<OperationResult<int>>` — ตรวจและบันทึกข้อมูลที่คลาสนี้รับผิดชอบ
  - [`UserAdminService.DemoteAdminAsync`](WebApplication1/Services/Backoffice/UserAdminService.cs#L106) `(int userId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจสิทธิ์ Owner ก่อนลดบทบาทแอดมิน
  - [`UserAdminService.SetSuspendedAsync`](WebApplication1/Services/Backoffice/UserAdminService.cs#L132) `(int userId, bool suspended, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — เปลี่ยนสถานะระงับบัญชีตามสิทธิ์ผู้ดำเนินการ
  - [`UserAdminService.DeleteAsync`](WebApplication1/Services/Backoffice/UserAdminService.cs#L159) `(int userId, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข

</details>

<details>
<summary><code>WebApplication1/Services/Catalog</code></summary>

- **[IProductCatalogService.cs](WebApplication1/Services/Catalog/IProductCatalogService.cs)**
  - [`IProductCatalogService.GetShopAsync`](WebApplication1/Services/Catalog/IProductCatalogService.cs#L8) `(int shopId, CancellationToken cancellationToken = default) → Task<StorefrontShop?>` — อ่านข้อมูลร้านและเตรียมหน้าสินค้าเฉพาะร้าน
  - [`IProductCatalogService.GetShopCategoryCountsAsync`](WebApplication1/Services/Catalog/IProductCatalogService.cs#L10) `(int shopId, CancellationToken cancellationToken = default) → Task<IReadOnlyList<CategoryCount>>` — นับจำนวนสินค้าในแต่ละหมวดตามขอบเขตที่ส่งมา
  - [`IProductCatalogService.GetCardsAsync`](WebApplication1/Services/Catalog/IProductCatalogService.cs#L12) `(ProductQuery query, CancellationToken cancellationToken = default) → Task<IReadOnlyList<ProductCard>>` — อ่านสินค้าและแปลงเป็นข้อมูลการ์ดที่หน้าเว็บใช้
  - [`IProductCatalogService.GetCardAsync`](WebApplication1/Services/Catalog/IProductCatalogService.cs#L14) `(int productId, CancellationToken cancellationToken = default) → Task<ProductCard?>` — อ่านสินค้าและแปลงเป็นข้อมูลการ์ดที่หน้าเว็บใช้
  - [`IProductCatalogService.GetDescriptionAsync`](WebApplication1/Services/Catalog/IProductCatalogService.cs#L16) `(int productId, CancellationToken cancellationToken = default) → Task<string?>` — อ่านรายละเอียดสินค้าตามรหัส
  - [`IProductCatalogService.GetCategoryCountsAsync`](WebApplication1/Services/Catalog/IProductCatalogService.cs#L18) `(CancellationToken cancellationToken = default) → Task<IReadOnlyList<CategoryCount>>` — นับจำนวนสินค้าในแต่ละหมวดตามขอบเขตที่ส่งมา
  - [`IProductCatalogService.GetStatsAsync`](WebApplication1/Services/Catalog/IProductCatalogService.cs#L20) `(CancellationToken cancellationToken = default) → Task<StoreStats>` — อ่านตัวเลขสรุปที่ใช้แสดงบนหน้าร้าน
- **[IReviewService.cs](WebApplication1/Services/Catalog/IReviewService.cs)**
  - [`IReviewService.GetForProductAsync`](WebApplication1/Services/Catalog/IReviewService.cs#L9) `(int productId, int? viewerUserId, CancellationToken cancellationToken = default) → Task<IReadOnlyList<ReviewItem>>` — ดึงรีวิวตามสินค้าหรือร้านที่อยู่ในขอบเขตของคำขอ
  - [`IReviewService.GetEligibilityAsync`](WebApplication1/Services/Catalog/IReviewService.cs#L11) `(int productId, int? userId, CancellationToken cancellationToken = default) → Task<ReviewEligibility>` — ตรวจประวัติออเดอร์เพื่อหาสินค้าที่ลูกค้ามีสิทธิ์รีวิว
  - [`IReviewService.SaveAsync`](WebApplication1/Services/Catalog/IReviewService.cs#L13) `(int userId, int productId, int rating, string? comment, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจและบันทึกข้อมูลที่คลาสนี้รับผิดชอบ
  - [`IReviewService.GetReviewableProductIdsAsync`](WebApplication1/Services/Catalog/IReviewService.cs#L16) `(int userId, CancellationToken cancellationToken = default) → Task<IReadOnlySet<int>>` — ตรวจประวัติออเดอร์เพื่อหาสินค้าที่ลูกค้ามีสิทธิ์รีวิว
- **[ProductCatalogService.cs](WebApplication1/Services/Catalog/ProductCatalogService.cs)**
  - [`ProductCatalogService.ProductCatalogService`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L18) `(AppDbContext db, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ProductCatalogService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ProductCatalogService.GetCardsAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L24) `(ProductQuery query, CancellationToken cancellationToken = default) → Task<IReadOnlyList<ProductCard>>` — อ่านสินค้าและแปลงเป็นข้อมูลการ์ดที่หน้าเว็บใช้
  - [`ProductCatalogService.GetCardAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L58) `(int productId, CancellationToken cancellationToken = default) → Task<ProductCard?>` — อ่านสินค้าและแปลงเป็นข้อมูลการ์ดที่หน้าเว็บใช้
  - [`ProductCatalogService.GetDescriptionAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L64) `(int productId, CancellationToken cancellationToken = default) → Task<string?>` — อ่านรายละเอียดสินค้าตามรหัส
  - [`ProductCatalogService.GetShopAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L67) `(int shopId, CancellationToken cancellationToken = default) → Task<StorefrontShop?>` — อ่านข้อมูลร้านและเตรียมหน้าสินค้าเฉพาะร้าน
  - [`ProductCatalogService.GetShopCategoryCountsAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L76) `(int shopId, CancellationToken cancellationToken = default) → Task<IReadOnlyList<CategoryCount>>` — นับจำนวนสินค้าในแต่ละหมวดตามขอบเขตที่ส่งมา
  - [`ProductCatalogService.GetCategoryCountsAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L79) `(CancellationToken cancellationToken = default) → Task<IReadOnlyList<CategoryCount>>` — นับจำนวนสินค้าในแต่ละหมวดตามขอบเขตที่ส่งมา
  - [`ProductCatalogService.CountCategoriesAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L82) `(IQueryable<Product> products, CancellationToken cancellationToken) → Task<IReadOnlyList<CategoryCount>>` — นับจำนวนสินค้าในแต่ละหมวดตามขอบเขตที่ส่งมา
  - [`ProductCatalogService.GetStatsAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L94) `(CancellationToken cancellationToken = default) → Task<StoreStats>` — อ่านตัวเลขสรุปที่ใช้แสดงบนหน้าร้าน
  - [`ProductCatalogService.ToCardsAsync`](WebApplication1/Services/Catalog/ProductCatalogService.cs#L102) `(IReadOnlyCollection<Product> products, CancellationToken cancellationToken) → Task<List<ProductCard>>` — อ่านสินค้าและแปลงเป็นข้อมูลการ์ดที่หน้าเว็บใช้
- **[ReviewService.cs](WebApplication1/Services/Catalog/ReviewService.cs)**
  - [`ReviewService.ReviewService`](WebApplication1/Services/Catalog/ReviewService.cs#L17) `(AppDbContext db, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ReviewService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ReviewService.GetForProductAsync`](WebApplication1/Services/Catalog/ReviewService.cs#L23) `(int productId, int? viewerUserId, CancellationToken cancellationToken = default) → Task<IReadOnlyList<ReviewItem>>` — ดึงรีวิวตามสินค้าหรือร้านที่อยู่ในขอบเขตของคำขอ
  - [`ReviewService.GetEligibilityAsync`](WebApplication1/Services/Catalog/ReviewService.cs#L37) `(int productId, int? userId, CancellationToken cancellationToken = default) → Task<ReviewEligibility>` — ตรวจประวัติออเดอร์เพื่อหาสินค้าที่ลูกค้ามีสิทธิ์รีวิว
  - [`ReviewService.SaveAsync`](WebApplication1/Services/Catalog/ReviewService.cs#L59) `(int userId, int productId, int rating, string? comment, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจสิทธิ์รีวิวสินค้าแล้วสร้างหรือแก้รีวิวของลูกค้าคนเดิม
  - [`ReviewService.GetReviewableProductIdsAsync`](WebApplication1/Services/Catalog/ReviewService.cs#L104) `(int userId, CancellationToken cancellationToken = default) → Task<IReadOnlySet<int>>` — ตรวจประวัติออเดอร์เพื่อหาสินค้าที่ลูกค้ามีสิทธิ์รีวิว
  - [`ReviewService.FindDeliveredOrderIdAsync`](WebApplication1/Services/Catalog/ReviewService.cs#L114) `(int userId, int productId, CancellationToken cancellationToken) → Task<int?>` — ตรวจประวัติออเดอร์เพื่อหาสินค้าที่ลูกค้ามีสิทธิ์รีวิว
  - [`ReviewService.RefreshProductRatingAsync`](WebApplication1/Services/Catalog/ReviewService.cs#L122) `(int productId, CancellationToken cancellationToken) → Task` — คำนวณคะแนนและจำนวนรีวิวของสินค้าใหม่หลังบันทึกรีวิว
  - [`ReviewService.MaskName`](WebApplication1/Services/Catalog/ReviewService.cs#L130) `(string name) → string` — ปรับข้อความก่อนแสดงหรือส่งออก เช่น ปิดบางส่วนของชื่อหรือตัดข้อความยาว

</details>

<details>
<summary><code>WebApplication1/Services/Dashboards</code></summary>

- **[AccountingDashboard.cs](WebApplication1/Services/Dashboards/AccountingDashboard.cs)**
  - [`AccountingDashboard.AccountingDashboard`](WebApplication1/Services/Dashboards/AccountingDashboard.cs#L47) `(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock, IAccountingService accounting) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ AccountingDashboard และรับค่าหรือบริการที่คลาสต้องใช้
  - [`AccountingDashboard.BuildAsync`](WebApplication1/Services/Dashboards/AccountingDashboard.cs#L62) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage?>` — เลือกชุดข้อมูลสำหรับหน้าที่ขอใน Dashboard
  - [`AccountingDashboard.OrderStatusAsync`](WebApplication1/Services/Dashboards/AccountingDashboard.cs#L111) `(DashboardRequest request, IReadOnlyList<DashboardMenuItem> menu, CancellationToken cancellationToken) → Task<DashboardPage>` — กรองเลขออเดอร์และสถานะจัดส่ง สรุปยอด แล้วกรองสถานะการเงินก่อนแบ่งหน้า
- **[AdminDashboard.cs](WebApplication1/Services/Dashboards/AdminDashboard.cs)**
  - [`AdminDashboard.AdminDashboard`](WebApplication1/Services/Dashboards/AdminDashboard.cs#L39) `(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ AdminDashboard และรับค่าหรือบริการที่คลาสต้องใช้
  - [`AdminDashboard.BuildAsync`](WebApplication1/Services/Dashboards/AdminDashboard.cs#L59) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage?>` — เลือกชุดข้อมูลสำหรับหน้าที่ขอใน Dashboard
  - [`AdminDashboard.FlashSalesAsync`](WebApplication1/Services/Dashboards/AdminDashboard.cs#L73) `(CancellationToken token) → Task<DashboardPage>` — เตรียมฟอร์มและรายการแคมเปญ Flash Sale
  - [`AdminDashboard.ShopsAsync`](WebApplication1/Services/Dashboards/AdminDashboard.cs#L87) `(CancellationToken token) → Task<DashboardPage>` — เตรียมรายชื่อร้านและบัญชีที่ใช้มอบหมายร้านหรือพื้นที่
  - [`AdminDashboard.OverviewAsync`](WebApplication1/Services/Dashboards/AdminDashboard.cs#L100) `(CancellationToken cancellationToken) → Task<DashboardPage>` — เตรียมข้อมูลภาพรวมของบทบาทนั้น
  - [`AdminDashboard.UsersAsync`](WebApplication1/Services/Dashboards/AdminDashboard.cs#L132) `(DashboardRequest request, CancellationToken cancellationToken) → Task<AdminUsersModel>` — ค้นหรือแสดงบัญชีผู้ใช้พร้อมข้อมูลสิทธิ์
  - [`AdminDashboard.CouponsAsync`](WebApplication1/Services/Dashboards/AdminDashboard.cs#L150) `(CancellationToken cancellationToken) → Task<AdminCouponsModel>` — เตรียมรายการคูปองที่เก็บไว้และประวัติคูปอง
  - [`AdminDashboard.MembersQuery`](WebApplication1/Services/Dashboards/AdminDashboard.cs#L158) `() → IQueryable<AdminUserRow>` — ค้นหรือแสดงบัญชีผู้ใช้พร้อมข้อมูลสิทธิ์
- **[CustomerDashboard.cs](WebApplication1/Services/Dashboards/CustomerDashboard.cs)**
  - [`CustomerDashboard.CustomerDashboard`](WebApplication1/Services/Dashboards/CustomerDashboard.cs#L32) `(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock, IReviewService reviews) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ CustomerDashboard และรับค่าหรือบริการที่คลาสต้องใช้
  - [`CustomerDashboard.CouponsAsync`](WebApplication1/Services/Dashboards/CustomerDashboard.cs#L53) `(int userId, CancellationToken cancellationToken) → Task<CustomerCouponsModel>` — เตรียมรายการคูปองที่เก็บไว้และประวัติคูปอง
  - [`CustomerDashboard.BuildAsync`](WebApplication1/Services/Dashboards/CustomerDashboard.cs#L71) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage?>` — เลือกชุดข้อมูลสำหรับหน้าที่ขอใน Dashboard
  - [`CustomerDashboard.SnapshotsAsync`](WebApplication1/Services/Dashboards/CustomerDashboard.cs#L137) `(IEnumerable<Order> orders, CancellationToken cancellationToken) → Task<IReadOnlyDictionary<int, ProductSnapshot>>` — อ่านรายละเอียดสินค้าปัจจุบันสำหรับใช้อ้างอิงจากประวัติออเดอร์
- **[DashboardBase.cs](WebApplication1/Services/Dashboards/DashboardBase.cs)**
  - [`DashboardBase.DashboardBase`](WebApplication1/Services/Dashboards/DashboardBase.cs#L20) `(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ DashboardBase และรับค่าหรือบริการที่คลาสต้องใช้
  - [`DashboardBase.BuildAsync`](WebApplication1/Services/Dashboards/DashboardBase.cs#L43) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage?>` — เลือกชุดข้อมูลสำหรับหน้าที่ขอใน Dashboard
  - [`DashboardBase.StartOfDayUtc`](WebApplication1/Services/Dashboards/DashboardBase.cs#L48) `(DateOnly day) → DateTime` — เตรียมช่วงวันและข้อมูลสรุปตามวันที่ของระบบ
  - [`DashboardBase.LocalDay`](WebApplication1/Services/Dashboards/DashboardBase.cs#L51) `(DateTime utc) → DateOnly` — เตรียมช่วงวันและข้อมูลสรุปตามวันที่ของระบบ
  - [`DashboardBase.Greeting`](WebApplication1/Services/Dashboards/DashboardBase.cs#L54) `() → string` — เตรียมคำทักทายสำหรับหน้า Dashboard
  - [`DashboardBase.Baht`](WebApplication1/Services/Dashboards/DashboardBase.cs#L60) `(decimal value) → string` — จัดรูปแบบค่าก่อนนำไปแสดงหรือส่งผ่านลิงก์
  - [`DashboardBase.Percent`](WebApplication1/Services/Dashboards/DashboardBase.cs#L62) `(decimal current, decimal previous) → string` — จัดรูปแบบค่าก่อนนำไปแสดงหรือส่งผ่านลิงก์
  - [`DashboardBase.Page`](WebApplication1/Services/Dashboards/DashboardBase.cs#L65) `(string section, string title, string partial, object? model, string? subtitle = null, GreetingBanner? greeting = null, IReadOnlyList<MetricTile>? metrics = null, IReadOnlyList<DashboardMenuItem>? menu = null) → DashboardPage` — ประกอบข้อมูลหน้า Dashboard หรือแบ่งข้อมูลเป็นหน้า
  - [`DashboardBase.PageAsync<T>`](WebApplication1/Services/Dashboards/DashboardBase.cs#L80) `(IQueryable<T> query, int page, CancellationToken cancellationToken) → Task<Paged<T>>` — ประกอบข้อมูลหน้า Dashboard หรือแบ่งข้อมูลเป็นหน้า
  - [`DashboardBase.RevenueSeriesAsync`](WebApplication1/Services/Dashboards/DashboardBase.cs#L90) `(int days, string label, CancellationToken cancellationToken, bool deliveredOnly = false, IQueryable<Order>? scopedOrders = null) → Task<ChartData>` — เตรียมช่วงวันและข้อมูลสรุปตามวันที่ของระบบ
  - [`DashboardBase.DeliveredBetween`](WebApplication1/Services/Dashboards/DashboardBase.cs#L110) `(DateOnly? from, DateOnly? to) → IQueryable<Order>` — เตรียมช่วงวันและข้อมูลสรุปตามวันที่ของระบบ
  - [`DashboardBase.SettlementSummaryAsync`](WebApplication1/Services/Dashboards/DashboardBase.cs#L128) `(IQueryable<Order> delivered, CancellationToken cancellationToken) → Task<SettlementSummary>` — เตรียมรายการและยอดรวมการจ่ายเงินตามขอบเขตออเดอร์
  - [`DashboardBase.SettlementRowsAsync`](WebApplication1/Services/Dashboards/DashboardBase.cs#L152) `(IQueryable<Order> delivered, int page, CancellationToken cancellationToken) → Task<Paged<SettlementRow>>` — เตรียมรายการและยอดรวมการจ่ายเงินตามขอบเขตออเดอร์
- **[IRoleDashboard.cs](WebApplication1/Services/Dashboards/IRoleDashboard.cs)**
  - [`IRoleDashboard.BuildAsync`](WebApplication1/Services/Dashboards/IRoleDashboard.cs#L15) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage?>` — เลือกชุดข้อมูลสำหรับหน้าที่ขอใน Dashboard
- **[OwnerDashboard.cs](WebApplication1/Services/Dashboards/OwnerDashboard.cs)**
  - [`OwnerDashboard.OwnerDashboard`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L28) `(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ OwnerDashboard และรับค่าหรือบริการที่คลาสต้องใช้
  - [`OwnerDashboard.BuildAsync`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L44) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage?>` — เลือกชุดข้อมูลสำหรับหน้าที่ขอใน Dashboard
  - [`OwnerDashboard.AdminAccessAsync`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L54) `(int page, CancellationToken cancellationToken) → Task<DashboardPage>` — อ่านรายการสำหรับจัดการสิทธิ์แอดมิน
  - [`OwnerDashboard.OverviewAsync`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L66) `(CancellationToken cancellationToken) → Task<DashboardPage>` — เตรียมข้อมูลภาพรวมของบทบาทนั้น
  - [`OwnerDashboard.SalesAsync`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L93) `(string? filter, CancellationToken cancellationToken) → Task<DashboardPage>` — เตรียมยอดขายและข้อมูลกราฟตามช่วงเวลา
  - [`OwnerDashboard.FinanceAsync`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L112) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage>` — เตรียมภาพรวมรายได้แพลตฟอร์มและยอดที่เกี่ยวข้อง
  - [`OwnerDashboard.RankingAsync`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L134) `(CancellationToken cancellationToken) → Task<DashboardPage>` — เตรียมข้อมูลสินค้าขายดีตามเกณฑ์จัดอันดับ
  - [`OwnerDashboard.PeriodAsync`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L141) `(DateTime from, DateTime to, CancellationToken cancellationToken) → Task<(int Orders, decimal Revenue)>` — เตรียมช่วงวันและข้อมูลสรุปตามวันที่ของระบบ
  - [`OwnerDashboard.TopProductsAsync`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L151) `(int take, CancellationToken cancellationToken) → Task<List<TopProduct>>` — เตรียมข้อมูลสินค้าขายดีตามเกณฑ์จัดอันดับ
  - [`OwnerDashboard.Shorten`](WebApplication1/Services/Dashboards/OwnerDashboard.cs#L163) `(string name) → string` — ปรับข้อความก่อนแสดงหรือส่งออก เช่น ปิดบางส่วนของชื่อหรือตัดข้อความยาว
- **[RaiderDashboard.cs](WebApplication1/Services/Dashboards/RaiderDashboard.cs)**
  - [`RaiderDashboard.RaiderDashboard`](WebApplication1/Services/Dashboards/RaiderDashboard.cs#L20) `(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ RaiderDashboard และรับค่าหรือบริการที่คลาสต้องใช้
  - [`RaiderDashboard.BuildAsync`](WebApplication1/Services/Dashboards/RaiderDashboard.cs#L35) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage?>` — เลือกชุดข้อมูลสำหรับหน้าที่ขอใน Dashboard
  - [`RaiderDashboard.JobsAsync`](WebApplication1/Services/Dashboards/RaiderDashboard.cs#L60) `(int riderId, IQueryable<Order> ready, IReadOnlyList<DashboardMenuItem> menu, CancellationToken token) → Task<DashboardPage>` — เตรียมงานที่ไรเดอร์มีสิทธิ์รับในพื้นที่รับผิดชอบ
  - [`RaiderDashboard.ReadyFor`](WebApplication1/Services/Dashboards/RaiderDashboard.cs#L70) `(int riderId) → IQueryable<Order>` — คัดงานที่พร้อมให้ไรเดอร์คนนี้รับตามพื้นที่และสถานะ
  - [`RaiderDashboard.OverviewAsync`](WebApplication1/Services/Dashboards/RaiderDashboard.cs#L76) `(int riderId, int available, int mine, IReadOnlyList<DashboardMenuItem> menu, CancellationToken cancellationToken) → Task<DashboardPage>` — เตรียมข้อมูลภาพรวมของบทบาทนั้น
  - [`RaiderDashboard.HistoryAsync`](WebApplication1/Services/Dashboards/RaiderDashboard.cs#L98) `(int riderId, DashboardRequest request, IReadOnlyList<DashboardMenuItem> menu, CancellationToken cancellationToken) → Task<DashboardPage>` — เตรียมประวัติงานจัดส่งและรายได้ของไรเดอร์
- **[ShopDashboard.cs](WebApplication1/Services/Dashboards/ShopDashboard.cs)**
  - [`ShopDashboard.ShopDashboard`](WebApplication1/Services/Dashboards/ShopDashboard.cs#L31) `(AppDbContext db, IOptions<CommerceOptions> commerce, ICurrentUserService currentUser, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ShopDashboard และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ShopDashboard.BuildAsync`](WebApplication1/Services/Dashboards/ShopDashboard.cs#L50) `(DashboardRequest request, CancellationToken cancellationToken) → Task<DashboardPage?>` — เลือกชุดข้อมูลสำหรับหน้าที่ขอใน Dashboard
  - [`ShopDashboard.OverviewAsync`](WebApplication1/Services/Dashboards/ShopDashboard.cs#L70) `(int toPack, IReadOnlyList<DashboardMenuItem> menu, CancellationToken cancellationToken) → Task<DashboardPage>` — เตรียมข้อมูลภาพรวมของบทบาทนั้น
  - [`ShopDashboard.QueueAsync`](WebApplication1/Services/Dashboards/ShopDashboard.cs#L101) `(string? filter, CancellationToken cancellationToken) → Task<ShopQueueModel>` — เตรียมรายการออเดอร์ที่ร้านต้องจัดเตรียมและแพ็ก
  - [`ShopDashboard.ProductsAsync`](WebApplication1/Services/Dashboards/ShopDashboard.cs#L117) `(DashboardRequest request, CancellationToken cancellationToken) → Task<ShopProductsModel>` — อ่านรายการสินค้าโดยใช้เงื่อนไขของหน้าปัจจุบัน
  - [`ShopDashboard.RevenueAsync`](WebApplication1/Services/Dashboards/ShopDashboard.cs#L126) `(int page, CancellationToken cancellationToken) → Task<ShopRevenueModel>` — เตรียมรายได้และสถานะยอดโอนของร้าน
  - [`ShopDashboard.ShippedAsync`](WebApplication1/Services/Dashboards/ShopDashboard.cs#L135) `(DashboardRequest request, CancellationToken cancellationToken) → Task<ShopShippedModel>` — แสดงออเดอร์ของร้านที่เริ่มส่งแล้ว

</details>

<details>
<summary><code>WebApplication1/Services/Identity</code></summary>

- **[HttpSessionAuth.cs](WebApplication1/Services/Identity/HttpSessionAuth.cs)**
  - [`HttpSessionAuth.HttpSessionAuth`](WebApplication1/Services/Identity/HttpSessionAuth.cs#L10) `(IHttpContextAccessor httpContextAccessor) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ HttpSessionAuth และรับค่าหรือบริการที่คลาสต้องใช้
  - [`HttpSessionAuth.SignIn`](WebApplication1/Services/Identity/HttpSessionAuth.cs#L19) `(User user, string role) → void` — จัดการการเข้าสู่ระบบและข้อมูลที่เก็บในเซสชัน
  - [`HttpSessionAuth.RefreshProfile`](WebApplication1/Services/Identity/HttpSessionAuth.cs#L26) `(User user) → void` — ปรับข้อมูลส่วนตัวและอัปเดตข้อมูลที่ใช้แสดง
  - [`HttpSessionAuth.SetAvatar`](WebApplication1/Services/Identity/HttpSessionAuth.cs#L33) `(string avatar) → void` — เปลี่ยนรูปประจำตัวและอัปเดตข้อมูลรูปของบัญชี
  - [`HttpSessionAuth.SetRole`](WebApplication1/Services/Identity/HttpSessionAuth.cs#L35) `(string role) → void` — เปลี่ยนบทบาทบัญชี และอัปเดตสิทธิ์ที่ระบบใช้
  - [`HttpSessionAuth.SignOut`](WebApplication1/Services/Identity/HttpSessionAuth.cs#L37) `() → void` — ล้างสถานะล็อกอินและจบเซสชันผู้ใช้
- **[IAuthSession.cs](WebApplication1/Services/Identity/IAuthSession.cs)**
  - [`IAuthSession.SignIn`](WebApplication1/Services/Identity/IAuthSession.cs#L10) `(User user, string role) → void` — จัดการการเข้าสู่ระบบและข้อมูลที่เก็บในเซสชัน
  - [`IAuthSession.RefreshProfile`](WebApplication1/Services/Identity/IAuthSession.cs#L13) `(User user) → void` — ปรับข้อมูลส่วนตัวและอัปเดตข้อมูลที่ใช้แสดง
  - [`IAuthSession.SetAvatar`](WebApplication1/Services/Identity/IAuthSession.cs#L15) `(string avatar) → void` — เปลี่ยนรูปประจำตัวและอัปเดตข้อมูลรูปของบัญชี
  - [`IAuthSession.SetRole`](WebApplication1/Services/Identity/IAuthSession.cs#L17) `(string role) → void` — เปลี่ยนบทบาทบัญชี และอัปเดตสิทธิ์ที่ระบบใช้
  - [`IAuthSession.SignOut`](WebApplication1/Services/Identity/IAuthSession.cs#L19) `() → void` — ล้างสถานะล็อกอินและจบเซสชันผู้ใช้
- **[ICurrentUserService.cs](WebApplication1/Services/Identity/ICurrentUserService.cs)**
  - [`ICurrentUserService.IsInRole`](WebApplication1/Services/Identity/ICurrentUserService.cs#L23) `(params string[] roles) → bool` — ตรวจว่าบทบาทของบัญชีตรงกับเงื่อนไขที่กำหนด
  - [`ICurrentUserService.RequireUserId`](WebApplication1/Services/Identity/ICurrentUserService.cs#L26) `() → int` — อ่านรหัสผู้ใช้ที่ล็อกอินสำหรับคำขอปัจจุบัน
- **[SessionCurrentUserService.cs](WebApplication1/Services/Identity/SessionCurrentUserService.cs)**
  - [`SessionCurrentUserService.SessionCurrentUserService`](WebApplication1/Services/Identity/SessionCurrentUserService.cs#L9) `(IHttpContextAccessor httpContextAccessor) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ SessionCurrentUserService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`SessionCurrentUserService.IsInRole`](WebApplication1/Services/Identity/SessionCurrentUserService.cs#L28) `(params string[] roles) → bool` — ตรวจว่าบทบาทของบัญชีตรงกับเงื่อนไขที่กำหนด
  - [`SessionCurrentUserService.RequireUserId`](WebApplication1/Services/Identity/SessionCurrentUserService.cs#L34) `() → int` — อ่านรหัสผู้ใช้ที่ล็อกอินสำหรับคำขอปัจจุบัน

</details>

<details>
<summary><code>WebApplication1/Services/Orders</code></summary>

- **[BasketCancellationService.cs](WebApplication1/Services/Orders/BasketCancellationService.cs)**
  - [`BasketCancellationService.CancelAsync`](WebApplication1/Services/Orders/BasketCancellationService.cs#L11) `(Guid paymentId, int actorId, string? reason, CancellationToken token) → Task<OperationResult<bool>>` — ยกเลิกยอดที่ยังไม่ยืนยันทั้งตะกร้า แล้วคืนสต็อกและสิทธิ์ที่มีบันทึกในธุรกรรมเดียว
- **[CheckoutService.cs](WebApplication1/Services/Orders/CheckoutService.cs)**
  - [`CheckoutService.CheckoutService`](WebApplication1/Services/Orders/CheckoutService.cs#L23) `( AppDbContext db, IPricingService pricing, IOrderNumberGenerator orderNumbers, IOrderWorkflow workflow, TimeProvider clock, ILogger<CheckoutService> logger) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ CheckoutService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`CheckoutService.PlaceOrderAsync`](WebApplication1/Services/Orders/CheckoutService.cs#L39) `(int userId, PlaceOrderRequest request, CancellationToken cancellationToken = default) → Task<OperationResult<PlacedOrder>>` — ตรวจคำขอสั่งซื้อ ยอดที่ยืนยัน และคำขอซ้ำก่อนบันทึกออเดอร์
  - [`CheckoutService.SaveOrderAsync`](WebApplication1/Services/Orders/CheckoutService.cs#L97) `( int userId, PlaceOrderRequest request, PaymentMethod paymentMethod, PricingQuote quote, string region, BankSlip? slip, CancellationToken cancellationToken) → Task<OperationResult<PlacedOrder>>` — บันทึกยอดชำระรวม แยกออเดอร์ตามร้าน และตัดสิทธิ์ที่ใช้ในธุรกรรมเดียว
  - [`CheckoutService.IsCouponEligible`](WebApplication1/Services/Orders/CheckoutService.cs#L221) `(PricedLine line) → bool` — ตรวจเงื่อนไขคูปองและสิทธิ์บัญชีก่อนคำนวณส่วนลด
  - [`CheckoutService.FindPaymentAsync`](WebApplication1/Services/Orders/CheckoutService.cs#L225) `(int userId, Guid paymentId, CancellationToken token) → Task<OperationResult<PlacedOrder>?>` — ค้นออเดอร์จากรหัสคำขอเดิมของลูกค้าและตรวจว่ายอดนั้นถูกยกเลิกแล้วหรือไม่
  - [`CheckoutService.IsDuplicateOrderNumber`](WebApplication1/Services/Orders/CheckoutService.cs#L241) `(DbUpdateException ex) → bool` — จำแนกข้อผิดพลาดจากฐานที่บ่งชี้การชนของเลขออเดอร์
- **[DailyOrderNumberGenerator.cs](WebApplication1/Services/Orders/DailyOrderNumberGenerator.cs)**
  - [`DailyOrderNumberGenerator.DailyOrderNumberGenerator`](WebApplication1/Services/Orders/DailyOrderNumberGenerator.cs#L19) `(AppDbContext db, IOptions<CommerceOptions> commerce, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ DailyOrderNumberGenerator และรับค่าหรือบริการที่คลาสต้องใช้
  - [`DailyOrderNumberGenerator.NextAsync`](WebApplication1/Services/Orders/DailyOrderNumberGenerator.cs#L26) `(CancellationToken cancellationToken = default) → Task<string>` — หาเลขลำดับถัดไปสำหรับสร้างเลขออเดอร์ของวันปัจจุบัน
- **[ICheckoutService.cs](WebApplication1/Services/Orders/ICheckoutService.cs)**
  - [`ICheckoutService.PlaceOrderAsync`](WebApplication1/Services/Orders/ICheckoutService.cs#L11) `(int userId, PlaceOrderRequest request, CancellationToken cancellationToken = default) → Task<OperationResult<PlacedOrder>>` — ตรวจคำขอสั่งซื้อ ยอดที่ยืนยัน และคำขอซ้ำก่อนบันทึกออเดอร์
- **[IOrderFulfillmentService.cs](WebApplication1/Services/Orders/IOrderFulfillmentService.cs)**
  - [`IOrderFulfillmentService.MoveToAsync`](WebApplication1/Services/Orders/IOrderFulfillmentService.cs#L12) `(string orderNo, OrderStatus nextStatus, CancellationToken cancellationToken = default) → Task<OperationResult<OrderStatus>>` — ตรวจบทบาทและเปลี่ยนสถานะออเดอร์ตามเส้นทางที่อนุญาต
  - [`IOrderFulfillmentService.CancelAsync`](WebApplication1/Services/Orders/IOrderFulfillmentService.cs#L15) `(string orderNo, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ยกเลิกรายการที่ยังเข้าเงื่อนไขและคืนสิทธิ์ในขอบเขตของรายการ
- **[IOrderNumberGenerator.cs](WebApplication1/Services/Orders/IOrderNumberGenerator.cs)**
  - [`IOrderNumberGenerator.NextAsync`](WebApplication1/Services/Orders/IOrderNumberGenerator.cs#L6) `(CancellationToken cancellationToken = default) → Task<string>` — ค้นคำขอเดิมหรือตรวจและสร้างเลขออเดอร์ที่ใช้งานได้
- **[IPricingService.cs](WebApplication1/Services/Orders/IPricingService.cs)**
  - [`IPricingService.QuoteAsync`](WebApplication1/Services/Orders/IPricingService.cs#L9) `(IReadOnlyCollection<CartLineRequest> items, string? couponCode, int? userId, CancellationToken cancellationToken = default, bool useRewards = false) → Task<PricingQuote>` — คำนวณยอดตะกร้าจากราคา สต็อก ส่วนลด และค่าส่งที่เซิร์ฟเวอร์ตรวจได้
- **[IShipmentService.cs](WebApplication1/Services/Orders/IShipmentService.cs)**
  - [`IShipmentService.DispatchAsync`](WebApplication1/Services/Orders/IShipmentService.cs#L13) `(string orderNo, ShipmentDetails shipment, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — บันทึกผู้ให้บริการและเลขพัสดุ แล้วเปลี่ยนออเดอร์เป็นกำลังจัดส่ง
- **[OrderFulfillmentService.cs](WebApplication1/Services/Orders/OrderFulfillmentService.cs)**
  - [`OrderFulfillmentService.OrderFulfillmentService`](WebApplication1/Services/Orders/OrderFulfillmentService.cs#L19) `(AppDbContext db, IOrderWorkflow workflow, ICurrentUserService currentUser, TimeProvider clock, ILogger<OrderFulfillmentService> logger) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ OrderFulfillmentService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`OrderFulfillmentService.MoveToAsync`](WebApplication1/Services/Orders/OrderFulfillmentService.cs#L28) `(string orderNo, OrderStatus nextStatus, CancellationToken cancellationToken = default) → Task<OperationResult<OrderStatus>>` — ตรวจบทบาทและเปลี่ยนสถานะออเดอร์ตามเส้นทางที่อนุญาต
  - [`OrderFulfillmentService.CancelAsync`](WebApplication1/Services/Orders/OrderFulfillmentService.cs#L101) `(string orderNo, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — ตรวจสิทธิ์และสถานะของออเดอร์เดี่ยวก่อนคืนสต็อกและคูปอง
  - [`OrderFulfillmentService.VisibleOrders`](WebApplication1/Services/Orders/OrderFulfillmentService.cs#L149) `() → IQueryable<Order>` — จำกัด Query ให้อยู่ในขอบเขตบัญชีหรือร้านที่มีสิทธิ์
- **[PricingService.cs](WebApplication1/Services/Orders/PricingService.cs)**
  - [`PricingService.IsFirstOrderCouponUnavailableAsync`](WebApplication1/Services/Orders/PricingService.cs#L14) `(AppDbContext db, string code, int userId, CancellationToken token) → Task<bool>` — ตรวจเงื่อนไขคูปองและสิทธิ์บัญชีก่อนคำนวณส่วนลด
  - [`PricingService.PricingService`](WebApplication1/Services/Orders/PricingService.cs#L23) `(AppDbContext db, IOptions<CommerceOptions> commerce, TimeProvider clock) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ PricingService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`PricingService.QuoteAsync`](WebApplication1/Services/Orders/PricingService.cs#L30) `(IReadOnlyCollection<CartLineRequest> items, string? couponCode, int? userId, CancellationToken cancellationToken = default, bool useRewards = false) → Task<PricingQuote>` — คำนวณยอดตะกร้าจากราคา สต็อก ส่วนลด และค่าส่งที่เซิร์ฟเวอร์ตรวจได้
  - [`PricingService.ApplyCouponAsync`](WebApplication1/Services/Orders/PricingService.cs#L93) `( string? couponCode, int? userId, IReadOnlyList<PricedLine> lines, decimal shippingFee, CancellationToken cancellationToken) → Task<(decimal Discount, decimal ShippingWaived, string? Code, string? Name, string? Error, IReadOnlyList<string>? Categories)>` — ตรวจเงื่อนไขคูปองและสิทธิ์บัญชีก่อนคำนวณส่วนลด
- **[SalesIncentives.cs](WebApplication1/Services/Orders/SalesIncentives.cs)**
  - [`SalesIncentives.ActiveFlashSales`](WebApplication1/Services/Orders/SalesIncentives.cs#L11) `(this AppDbContext db, DateTime now) → IQueryable<FlashSale>` — เลือกแคมเปญที่ถึงเวลาและยังมีโควตา
  - [`SalesIncentives.LoyaltyBalanceAsync`](WebApplication1/Services/Orders/SalesIncentives.cs#L14) `(this AppDbContext db, int customerId, CancellationToken token) → Task<LoyaltyBalance>` — คำนวณยอดเงินสะสมที่ใช้ได้และยอดคืบหน้าไปยังบาทถัดไป
- **[ShipmentService.cs](WebApplication1/Services/Orders/ShipmentService.cs)**
  - [`ShipmentService.ShipmentService`](WebApplication1/Services/Orders/ShipmentService.cs#L20) `(AppDbContext db, ICurrentUserService currentUser, TimeProvider clock, ILogger<ShipmentService> logger) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ ShipmentService และรับค่าหรือบริการที่คลาสต้องใช้
  - [`ShipmentService.DispatchAsync`](WebApplication1/Services/Orders/ShipmentService.cs#L28) `(string orderNo, ShipmentDetails shipment, CancellationToken cancellationToken = default) → Task<OperationResult<bool>>` — บันทึกผู้ให้บริการและเลขพัสดุ แล้วเปลี่ยนออเดอร์เป็นกำลังจัดส่ง
  - [`ShipmentService.Validate`](WebApplication1/Services/Orders/ShipmentService.cs#L97) `(Order? order) → string?` — ตรวจข้อมูลนำเข้าและคืนเหตุผลเมื่อพบข้อมูลผิดเงื่อนไข
  - [`ShipmentService.TrackingNumberTakenAsync`](WebApplication1/Services/Orders/ShipmentService.cs#L108) `(ShipmentDetails shipment, CancellationToken cancellationToken) → Task<bool>` — ตรวจความซ้ำหรือรูปแบบของเลขติดตามพัสดุ
- **[SlipImage.cs](WebApplication1/Services/Orders/SlipImage.cs)**
  - [`SlipImage.ReadAsync`](WebApplication1/Services/Orders/SlipImage.cs#L11) `(IFormFile? file, CancellationToken token, int maxBytes = MaxBytes) → Task<BankSlip?>` — อ่านไฟล์สลิปภายในขนาดที่กำหนดแล้วตรวจลายเซ็นของรูป
  - [`SlipImage.Decode`](WebApplication1/Services/Orders/SlipImage.cs#L20) `(string? base64) → BankSlip?` — อ่านข้อมูลสลิปจากข้อความหรือไบต์ แล้วตรวจชนิดรูปและเตรียมข้อมูลจัดเก็บ
  - [`SlipImage.FromBytes`](WebApplication1/Services/Orders/SlipImage.cs#L34) `(byte[] bytes) → BankSlip?` — อ่านข้อมูลสลิปจากข้อความหรือไบต์ แล้วตรวจชนิดรูปและเตรียมข้อมูลจัดเก็บ

</details>

<details>
<summary><code>WebApplication1/Services/Storage</code></summary>

- **[IImageStorage.cs](WebApplication1/Services/Storage/IImageStorage.cs)**
  - [`IImageStorage.SaveAsync`](WebApplication1/Services/Storage/IImageStorage.cs#L9) `(IFormFile file, string folder, CancellationToken cancellationToken = default) → Task<OperationResult<string>>` — ตรวจและบันทึกข้อมูลที่คลาสนี้รับผิดชอบ
  - [`IImageStorage.Delete`](WebApplication1/Services/Storage/IImageStorage.cs#L11) `(string? publicUrl) → void` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข
- **[LocalImageStorage.cs](WebApplication1/Services/Storage/LocalImageStorage.cs)**
  - [`LocalImageStorage.LocalImageStorage`](WebApplication1/Services/Storage/LocalImageStorage.cs#L28) `(IWebHostEnvironment environment, ILogger<LocalImageStorage> logger) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ LocalImageStorage และรับค่าหรือบริการที่คลาสต้องใช้
  - [`LocalImageStorage.RootFor`](WebApplication1/Services/Storage/LocalImageStorage.cs#L35) `(IWebHostEnvironment environment) → string` — คำนวณตำแหน่งโฟลเดอร์หลักสำหรับไฟล์อัปโหลด
  - [`LocalImageStorage.SaveAsync`](WebApplication1/Services/Storage/LocalImageStorage.cs#L37) `(IFormFile file, string folder, CancellationToken cancellationToken = default) → Task<OperationResult<string>>` — ตรวจขนาดและชนิดไฟล์ก่อนบันทึกลงโฟลเดอร์อัปโหลดและคืน URL
  - [`LocalImageStorage.Delete`](WebApplication1/Services/Storage/LocalImageStorage.cs#L68) `(string? publicUrl) → void` — ลบรายการในขอบเขตที่คลาสรับผิดชอบตามเงื่อนไข

</details>

<details>
<summary><code>WebApplication1/TagHelpers</code></summary>

- **[ActiveRouteTagHelper.cs](WebApplication1/TagHelpers/ActiveRouteTagHelper.cs)**
  - [`ActiveRouteTagHelper.Process`](WebApplication1/TagHelpers/ActiveRouteTagHelper.cs#L24) `(TagHelperContext context, TagHelperOutput output) → void` — ประมวลผล Tag Helper เพื่อปรับส่วนที่แสดงใน HTML
  - [`ActiveRouteTagHelper.Matches`](WebApplication1/TagHelpers/ActiveRouteTagHelper.cs#L35) `(string? controller, string? action) → bool` — เปรียบเทียบ Controller และ Action กับค่าที่กำหนด

</details>

<details>
<summary><code>WebApplication1/ViewComponents</code></summary>

- **[CheckoutFormViewComponent.cs](WebApplication1/ViewComponents/CheckoutFormViewComponent.cs)**
  - [`CheckoutFormViewComponent.CheckoutFormViewComponent`](WebApplication1/ViewComponents/CheckoutFormViewComponent.cs#L25) `(ICurrentUserService currentUser, IAddressService addresses, AppDbContext db) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ CheckoutFormViewComponent และรับค่าหรือบริการที่คลาสต้องใช้
  - [`CheckoutFormViewComponent.InvokeAsync`](WebApplication1/ViewComponents/CheckoutFormViewComponent.cs#L32) `() → Task<IViewComponentResult>` — เตรียมข้อมูลให้ส่วนที่ใช้ซ้ำหรือทำงานใน middleware ตามชนิดคลาส
- **[FooterViewComponent.cs](WebApplication1/ViewComponents/FooterViewComponent.cs)**
  - [`FooterViewComponent.FooterViewComponent`](WebApplication1/ViewComponents/FooterViewComponent.cs#L21) `(IOptions<StoreInfoOptions> store, IOptions<CommerceOptions> commerce) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ FooterViewComponent และรับค่าหรือบริการที่คลาสต้องใช้
  - [`FooterViewComponent.Invoke`](WebApplication1/ViewComponents/FooterViewComponent.cs#L27) `(bool compact = false) → IViewComponentResult` — เตรียมข้อมูลให้ส่วนที่ใช้ซ้ำหรือทำงานใน middleware ตามชนิดคลาส
- **[LayoutVariant.cs](WebApplication1/ViewComponents/LayoutVariant.cs)**
  - [`LayoutVariantResolver.From`](WebApplication1/ViewComponents/LayoutVariant.cs#L17) `(IDictionary<string, object?> viewData) → LayoutVariant` — แปลงหรือคัดลอกข้อมูลจากชนิดต้นทางไปยังชนิดที่ส่วนถัดไปใช้
- **[NavbarViewComponent.cs](WebApplication1/ViewComponents/NavbarViewComponent.cs)**
  - [`NavbarViewComponent.NavbarViewComponent`](WebApplication1/ViewComponents/NavbarViewComponent.cs#L20) `(ICurrentUserService currentUser) → ตัวสร้างออบเจ็กต์` — สร้างออบเจ็กต์ NavbarViewComponent และรับค่าหรือบริการที่คลาสต้องใช้
  - [`NavbarViewComponent.Invoke`](WebApplication1/ViewComponents/NavbarViewComponent.cs#L25) `(bool compact = false) → IViewComponentResult` — เตรียมข้อมูลให้ส่วนที่ใช้ซ้ำหรือทำงานใน middleware ตามชนิดคลาส

</details>

<details>
<summary><code>WebApplication1/ViewModels/Account</code></summary>

- **[ProfileModels.cs](WebApplication1/ViewModels/Account/ProfileModels.cs)**
  - [`AddressForm.ToEntity`](WebApplication1/ViewModels/Account/ProfileModels.cs#L55) `() → UserAddress` — แปลงหรือคัดลอกข้อมูลจากชนิดต้นทางไปยังชนิดที่ส่วนถัดไปใช้

</details>

<details>
<summary><code>WebApplication1/ViewModels/Checkout</code></summary>

- **[CheckoutRequests.cs](WebApplication1/ViewModels/Checkout/CheckoutRequests.cs)**
  - [`CartLineInput.ToRequest`](WebApplication1/ViewModels/Checkout/CheckoutRequests.cs#L15) `() → CartLineRequest` — แปลงหรือคัดลอกข้อมูลจากชนิดต้นทางไปยังชนิดที่ส่วนถัดไปใช้

</details>

<details>
<summary><code>WebApplication1/ViewModels/Dashboard</code></summary>

- **[DashboardModels.cs](WebApplication1/ViewModels/Dashboard/DashboardModels.cs)**
  - [`PagerModel.Iso`](WebApplication1/ViewModels/Dashboard/DashboardModels.cs#L70) `(DateOnly? date) → string?` — จัดรูปแบบค่าก่อนนำไปแสดงหรือส่งผ่านลิงก์
  - [`PagerModel.From<T>`](WebApplication1/ViewModels/Dashboard/DashboardModels.cs#L72) `(Paged<T> paged, string section, string? search = null, string? filter = null) → PagerModel` — แปลงหรือคัดลอกข้อมูลจากชนิดต้นทางไปยังชนิดที่ส่วนถัดไปใช้
- **[DashboardRoutes.cs](WebApplication1/ViewModels/Dashboard/DashboardRoutes.cs)**
  - [`DashboardRoutes.ActionFor`](WebApplication1/ViewModels/Dashboard/DashboardRoutes.cs#L5) `(string? section) → string` — แปลงชื่อเมนู Dashboard เป็นชื่อ Action สำหรับสร้างลิงก์
- **[StatusTone.cs](WebApplication1/ViewModels/Dashboard/StatusTone.cs)**
  - [`StatusTone.CssClass`](WebApplication1/ViewModels/Dashboard/StatusTone.cs#L8) `(OrderStatus status) → string` — คืนชื่อคลาสสไตล์ที่สัมพันธ์กับข้อมูล

</details>

<details>
<summary><code>WebApplication1/ViewModels/Shared</code></summary>

- **[ShipmentInfo.cs](WebApplication1/ViewModels/Shared/ShipmentInfo.cs)**
  - [`ShipmentInfo.For`](WebApplication1/ViewModels/Shared/ShipmentInfo.cs#L26) `(bool? riderDelivery, OrderStatus status, OrderShipment? shipment) → ShipmentInfo?` — เลือกข้อมูลประกอบจากค่าที่ส่งมา เช่น ข้อมูล Role หรือรายละเอียดการจัดส่ง
  - [`ShipmentInfo.For`](WebApplication1/ViewModels/Shared/ShipmentInfo.cs#L42) `(Order order) → ShipmentInfo?` — เลือกข้อมูลประกอบจากค่าที่ส่งมา เช่น ข้อมูล Role หรือรายละเอียดการจัดส่ง

</details>

<!-- csharp-method-index:end -->
