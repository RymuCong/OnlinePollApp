Hướng dẫn chạy migration dự án
1. Chỉnh sửa lại appsettings trong project FDS.CRM.WebApi, Lưu ý sửa các thông tin chính xác để kết nối tới SQL server
2. Set project FDS.CRM.Migration là start project
3. Chạy project này để tự động tạo database và seed data những bảng cần thiết
4. Kiểm tra lại SQL server xem đã tạo DB và data chưa, nếu chưa thì có thể chạy lệnh migration bằng tay để kiểm tra lỗi

HƯớng dẫn khởi chạy dự án
1. Sau khi khời tạo DB thành công thì set  start project là Project FDS.CRM.WebApi
2. Chạy FDS.CRM.WebApi để lên được giao diện swagger
3. Debug từng API để hiểu 
	
