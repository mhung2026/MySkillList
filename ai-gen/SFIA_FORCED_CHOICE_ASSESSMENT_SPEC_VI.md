# 🧠 TIÊU CHUẨN ĐÁNH GIÁ CẤP ĐØ KỸ NĂNG SFIA THEO HÀNH VI (AI-READY)

## 0. MỤC ĐÍCH

Tài liệu này định nghĩa **các quy tắc bắt buộc** để tạo, đánh giá và xác định
**cấp độ kỹ năng SFIA hiện tại (L1–L7)** của một người bằng cách sử dụng **câu hỏi tình huống dựa trên hành vi**.

Đánh giá PHẢI:
- Đo lường **xu hướng hành vi thực tế**
- Ngăn chặn câu trả lời "mong muốn xã hội" hoặc "gian lận"
- Suy luận level dựa trên **mẫu hình trách nhiệm nhất quán**

Tiêu chuẩn này được thiết kế cho **việc tạo test và suy luận bằng AI**.

---

## 1. NGUYÊN TẮC CỐT LÕI

> **Cấp độ SFIA = cấp độ trách nhiệm cao nhất mà người đó nhất quán lựa chọn đảm nhận trong các tình huống thực tế.**

Đánh giá PHẢI:
- Tránh kiểm tra kiến thức hoặc lý thuyết
- Tránh khung "đạo đức" hoặc "thực hành tốt nhất"
- Đo lường điều người đó có khả năng LÀM dưới các ràng buộc

---

## 2. CẤU TRÚC CÂU HỎI (BẮT BUỘC)

Mỗi câu hỏi PHẢI bao gồm TẤT CẢ các thành phần dưới đây.

### 2.1 Ngữ Cảnh Cố Định

Mỗi tình huống PHẢI định nghĩa rõ ràng:
- Loại task (thực thi / phối hợp / quyết định / chiến lược)
- Mức độ rủi ro (thấp / trung bình / cao)
- Áp lực thời gian (không có / vừa phải / khẩn cấp)
- Điều kiện quyền hạn (có leader / không có leader)
- Độ rõ ràng phạm vi (rõ ràng / mơ hồ)

❌ Cấm:
- Cách diễn đạt giả định hoặc mong muốn
- "Nếu bạn muốn…"

✅ Yêu cầu:
- Ngữ cảnh vận hành cụ thể, thực tế

---

### 2.2 Hướng Dẫn Forced-Choice

Hướng dẫn PHẢI là:

> **Chọn hành động mà bạn CÓ KHẢ NĂNG NHẤT sẽ làm trong thực tế.**

Từ cấm:
- nên
- tốt nhất
- đúng
- lý tưởng

---

### 2.3 Chính Xác 4 Lựa Chọn

Mỗi câu hỏi PHẢI có:
- Chính xác 4 lựa chọn
- Mỗi lựa chọn đại diện cho **một hành vi chủ đạo**
- Mỗi lựa chọn được ánh xạ trước với **chỉ một cấp độ SFIA**

Không cho phép hành vi hỗn hợp.

---

## 3. ĐẶC TRƯNG HÀNH VI THEO CẤP ĐỘ SFIA (L1–L7)

| Cấp độ | Hành Vi Cốt Lõi |
|-----|----------------|
| L1 – Follow | Chờ hướng dẫn, tránh quyết định |
| L2 – Assist | Làm theo chỉ dẫn, tìm kiếm xác nhận |
| L3 – Apply | Hành động độc lập trong phạm vi xác định |
| L4 – Enable | Chịu trách nhiệm về cách làm và chất lượng |
| L5 – Ensure / Advise | Ảnh hưởng người khác, đảm bảo tính nhất quán |
| L6 – Initiate / Influence | Khởi xướng thay đổi trong điều kiện không chắc chắn |
| L7 – Set Strategy / Inspire | Đặt ra định hướng và tầm nhìn dài hạn |

Mỗi lựa chọn PHẢI ánh xạ với **chính xác một cấp độ ở trên**.

---

## 4. YÊU CẦU TRADE-OFF (QUAN TRỌNG)

Mỗi câu hỏi PHẢI có ít nhất một sự đánh đổi thực sự:

- Tự chủ ↔ An toàn
- Tốc độ ↔ Chất lượng
- Trách nhiệm ↔ Leo thang
- Chủ động ↔ Tuân thủ
- Tối ưu cục bộ ↔ Tác động tổ chức

Nếu không có trade-off, câu hỏi là KHÔNG HỢP LỆ.

---

## 5. YÊU CẦU COVERAGE THEO CẤP ĐỘ

### 5.1 Bằng Chứng Tối Thiểu Cho Mỗi Cấp Độ

Một cấp độ chỉ có thể được suy luận nếu:
- Ít nhất **4 câu hỏi** ánh xạ đến cấp độ đó
- Trải rộng trên **ít nhất 3 ngữ cảnh khác biệt**

### 5.2 Loại Ngữ Cảnh

Ngữ cảnh PHẢI thay đổi trên:
- Rủi ro thấp vs rủi ro cao
- Phạm vi rõ ràng vs phạm vi mơ hồ
- Task cá nhân vs tác động nhiều người
- Hậu quả ngắn hạn vs dài hạn

---

## 6. CƠ CHẾ CHỐNG GIAN LẬN

### 6.1 Câu Hỏi Đảo Chiều

Ít nhất **25% câu hỏi** PHẢI:
- Đảo ngược ngữ cảnh hoặc rủi ro
- Kiểm tra xem hành vi có giữ nguyên không

Sự không nhất quán làm giảm độ tin cậy.

---

### 6.2 Kiểm Tra Chuyển Đổi Ngữ Cảnh

Cùng một hành vi PHẢI được kiểm tra trong:
- Các điều kiện quyền hạn khác nhau
- Các mức độ rủi ro khác nhau

Hành vi sụp đổ dưới áp lực = cấp độ thấp hơn.

---

## 7. QUY TẮC XÁC NHẬN CẤP ĐỘ (KHÔNG CHẤM ĐIỂM)

### 7.1 Không Có Điểm Số

AI KHÔNG ĐƯỢC:
- Gán điểm
- Tính trung bình
- Xếp hạng câu trả lời theo số

---

### 7.2 Tiêu Chí Xác Nhận Cho Cấp Độ L

Một cấp độ L được **XÁC NHẬN** nếu TẤT CẢ điều kiện sau được đáp ứng:

1. **Tính Nhất Quán**
   ≥ 70% câu trả lời ánh xạ đến cấp độ L được chọn

2. **Coverage Ngữ Cảnh**
   Câu trả lời xuất hiện trong ≥ 3 loại ngữ cảnh khác biệt

3. **Không Mâu Thuẫn**
   Không có xu hướng quay về cấp độ L-1 trong các ngữ cảnh tương đương

---

### 7.3 Xác Định Cấp Độ Cuối Cùng
Cấp độ cuối cùng = Cấp độ SFIA cao nhất đáp ứng tất cả tiêu chí xác nhận
Các cấp độ PHẢI được đánh giá từ trên xuống:
L7 → L6 → L5 → L4 → L3 → L2 → L1

Không cho phép tính trung bình giữa các cấp độ.

---

## 8. QUY TẮC VÔ HIỆU HÓA CẤP ĐỘ

Một cấp độ PHẢI bị từ chối nếu:

- Hành vi chỉ xuất hiện trong ngữ cảnh rủi ro thấp
- Câu hỏi đảo chiều mâu thuẫn với hành vi
- Hành vi cấp cao hơn biến mất dưới sự mơ hồ hoặc áp lực

---

## 9. OUTPUT BÁO CÁO (THEO TỪNG KỸ NĂNG)

AI PHẢI xuất ra:

1. Tên kỹ năng
2. Cấp độ SFIA suy luận
3. Bằng chứng hành vi (các gạch đầu dòng)
4. Giới hạn quan sát được
5. Khuyến nghị phát triển

### Ngôn ngữ cấm:
- Lười biếng
- Yếu
- Không có động lực
- Đánh giá dựa trên tính cách

Chỉ cho phép hành vi quan sát được.

---

## 10. KHẲNG ĐỊNH CUỐI CÙNG

Nếu tiêu chuẩn này được tuân thủ:
- Test đo lường **hành vi vận hành thực tế**
- Các cấp độ SFIA là **có thể bảo vệ và kiểm toán được**
- Ứng viên không thể giả mạo cấp độ cao hơn một cách đáng tin cậy

Vi phạm tiêu chuẩn này làm mất hiệu lực của đánh giá.
