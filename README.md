# Hệ thống Quản lý Nhân viên

Đây là một ứng dụng desktop Windows Forms để quản lý thông tin nhân viên, phòng ban và lương. Ứng dụng được xây dựng bằng C# và .NET Framework 4.7.2, với dữ liệu được lưu trữ trong cơ sở dữ liệu SQL Server.

## Tính năng

*   **Quản lý Nhân viên:** Thêm, cập nhật và xóa hồ sơ nhân viên.
*   **Quản lý Phòng ban & Lương:** Quản lý chi tiết phòng ban và lương cho nhân viên.
*   **Tài khoản Người dùng:** Hệ thống đăng nhập an toàn với mã hóa mật khẩu.
*   **Khôi phục Mật khẩu:** Chức năng "Quên mật khẩu" sử dụng OTP qua email.
*   **Báo cáo:** Tạo và xuất các báo cáo lương chi tiết của nhân viên dưới dạng PDF.
*   **Trực quan hóa Dữ liệu:** Xem các biểu đồ thống kê liên quan đến dữ liệu nhân viên.

## Yêu cầu cài đặt

Trước khi bắt đầu, hãy đảm bảo bạn đã cài đặt các công cụ sau:
*   **Visual Studio 2022** (với workload ".NET desktop development")
*   **.NET Framework 4.7.2 Developer Pack**
*   **SQL Server** (bất kỳ phiên bản nào, như Express, Developer, hoặc Standard)

## Cài đặt và Cấu hình

### 1. Tải mã nguồn từ Repository

copy đường dẫn sau và dán vào Terminal

git clone https://github.com/TonyTheSlacker/EMPNhom4.git cd EMPNhom4


### 2. Cài đặt Cơ sở dữ liệu

Ứng dụng yêu cầu một cơ sở dữ liệu SQL Server có tên là `EmployeeManagementSystem`.

1.  Mở SQL Server Management Studio (SSMS) và kết nối với SQL Server instance.
2.  Tạo một cơ sở dữ liệu mới, trống với tên là `EmployeeManagementSystem`.
3.  Ứng dụng sử dụng LINQ to SQL để tương tác với các bảng. Bạn cần tạo schema cho cơ sở dữ liệu (bảng, cột, v.v.). Nếu bạn có một tệp kịch bản `.sql`, hãy chạy nó để tạo schema. Nếu không, schema được định nghĩa trong tệp `Employee.dbml` của dự án.

### 3. Cấu hình Chuỗi kết nối

Bạn phải cập nhật chuỗi kết nối cơ sở dữ liệu để trỏ đến SQL Server instance trên máy của bạn.

1.  Mở file `EmployeeManagementSystem.sln` trong Visual Studio.
2.  Trong Solution Explorer, mở tệp `App.config`.
3.  Tìm đến phần `<connectionStrings>` và tìm chuỗi kết nối có tên `EmployeeManagementSystem.Properties.Settings.EmployeeManagementSystemConnectionString`.
4.  Sửa đổi `Data Source` để khớp với tên SQL Server instance của bạn. Đối với cài đặt SQL Express mặc định, tên này thường là `.\SQLEXPRESS` hoặc `(localdb)\MSSQLLocalDB`.

    **Ví dụ:** Thay đổi từ:
    ```xml
    <add name="EmployeeManagementSystem.Properties.Settings.EmployeeManagementSystemConnectionString"
        connectionString="Data Source=SURFACE\MYSQL;Initial Catalog=EmployeeManagementSystem;Integrated Security=True"
        providerName="System.Data.SqlClient" />
    ```
    Thành (nếu instanc là `SQLEXPRESS`):
    ```xml
    <add name="EmployeeManagementSystem.Properties.Settings.EmployeeManagementSystemConnectionString"
        connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=EmployeeManagementSystem;Integrated Security=True"
        providerName="System.Data.SqlClient" />
    ```

### 4. Cấu hình Email để khôi phục mật khẩu

Chức năng "Quên mật khẩu" sử dụng một tài khoản Gmail để gửi mã OTP. Người dùng phải cài đặt lại với email cá nhân của mình.

1.  Trong Solution Explorer, mở tệp `ForgetPasswordForm.cs`.
2.  Đi đến phương thức `btnRequest_Click`.
3.  **Cập nhật địa chỉ email người gửi:**
    Thay đổi địa chỉ email ở dòng này thành tài khoản Gmail của bạn:
    ```csharp
    var from = new MailAddress("example@gmail.com");
    ```

4.  **Cập nhật mật khẩu ứng dụng:**
    Bạn cần tạo một "Mật khẩu ứng dụng" gồm 16 ký tự từ Tài khoản Google của mình. **Không sử dụng mật khẩu thông thường của bạn.**
    *   Truy cập cài đặt Tài khoản Google của bạn (`myaccount.google.com`).
    *   Bật Xác minh 2 bước nếu bạn chưa bật.
    *   Đi đến tab "Bảo mật" và tìm "Mật khẩu ứng dụng".
    *   Tạo một mật khẩu ứng dụng mới cho ứng dụng này.
    *   Sao chép mật khẩu 16 ký tự (không có khoảng trắng) và dán vào dòng sau, thay thế cho chuỗi hiện tại:
    ```csharp
    const string frompass = "dddd ffff eeee wwww"; // Thay thế chuỗi này bằng Mật khẩu ứng dụng Google 16 ký tự trong app
    ```

### 5. Build và Chạy ứng dụng

1.  Trong Visual Studio, build solution bằng cách chọn __Build > Build Solution__ từ menu.
2.  Sau khi build thành công, chạy ứng dụng bằng cách nhấn **F5** hoặc nhấp vào nút "Start".

## Hướng dẫn sử dụng

1.  **Đăng nhập:** Khởi chạy ứng dụng để mở màn hình đăng nhập. Sử dụng thông tin đăng nhập hợp lệ để vào hệ thống.
2.  **Bảng điều khiển chính:** Sau khi đăng nhập, người dùng có thể điều hướng giữa các mục quản lý khác nhau (Nhân viên, Phòng ban, Lương, v.v.) bằng menu chính.
3.  **Báo cáo:** Điều hướng đến mục "Báo cáo" để xem, lọc và xuất báo cáo lương của nhân viên.
