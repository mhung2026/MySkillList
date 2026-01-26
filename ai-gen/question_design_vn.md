# 🧠 ĐẶC TẢ SINH CÂU HỎI FORCED-CHOICE (THEO SFIA)

## 0. MỤC ĐÍCH TÀI LIỆU

Tài liệu này định nghĩa **các quy tắc BẮT BUỘC** để sinh câu hỏi dạng forced-choice  
(Situational Judgement Test – SJT) nhằm đánh giá **level kỹ năng theo SFIA dựa trên HÀNH VI THỰC TẾ**.

Mục tiêu:
- Bóc tách **hành vi vận hành thật**
- Chống việc “làm bài cho đẹp”
- Suy luận level kỹ năng **có thể bảo vệ được**

Tài liệu này được viết để **AI có thể dựa vào đó sinh câu hỏi đúng chuẩn**.

---

## 1. NGUYÊN TẮC CỐT LÕI

> **Đánh giá điều người đó CÓ KHẢ NĂNG LÀM khi được giao việc,  
> không phải điều họ BIẾT LÀ ĐÚNG.**

Bài test:
- KHÔNG đánh giá đạo đức
- KHÔNG đánh giá kiến thức
- KHÔNG hỏi “best practice”

---

## 2. CẤU TRÚC BẮT BUỘC CỦA MỖI CÂU HỎI

Mỗi câu hỏi **PHẢI** có đầy đủ các thành phần sau:

---

### 2.1 Bối cảnh cố định (Fixed Context)

Bối cảnh phải mô tả rõ:
- Loại công việc (sửa bug, làm feature, phân tích, phối hợp, ra quyết định…)
- Ràng buộc (deadline, tài liệu, mức độ rủi ro)
- Tình trạng cấp trên (có sẵn / không sẵn)
- Phạm vi công việc (rõ / mơ hồ)

❌ Sai:
> Nếu bạn muốn…

✅ Đúng:
> Deadline trong ngày, có tài liệu, leader đang bận.

---

### 2.2 Câu lệnh Forced-choice

Câu lệnh **bắt buộc** phải dùng dạng:

> **Chọn hành vi bạn CÓ KHẢ NĂNG làm nhất trong thực tế**

❌ CẤM dùng:
- nên
- đúng
- chuẩn
- tốt nhất
- best practice

---

### 2.3 Chính xác 4 đáp án

- Không ít hơn
- Không nhiều hơn
- Mỗi đáp án đại diện cho **1 chiến lược hành vi riêng biệt**

---

## 3. QUY TẮC THIẾT KẾ ĐÁP ÁN (RẤT QUAN TRỌNG)

### 3.1 Không có đáp án “đúng”

- Không được có đáp án quá đẹp
- Không được có đáp án ngu ngốc

👉 Tất cả đều **có lý trong một số hoàn cảnh**

---

### 3.2 Mỗi đáp án = 1 hành vi CHỦ ĐẠO

❌ Không được trộn hành vi

| Chiều hành vi | Ví dụ |
|--------------|------|
| Phụ thuộc | Chờ phê duyệt |
| Tự chủ | Làm trong phạm vi |
| Né rủi ro | Tránh quyết định |
| Ưu tiên tốc độ | Làm trước, kiểm soát sau |
| Dựa đồng nghiệp | Tìm consensus |

---

### 3.3 Mapping Level SFIA (bắt buộc)

| Level SFIA | Dấu hiệu hành vi |
|-----------|----------------|
| L1 | Cần giám sát chặt |
| L2 | Làm theo hướng dẫn, hay xác nhận |
| L3 | Tự làm trong phạm vi rõ |
| L4 | Chịu trách nhiệm về cách làm & chất lượng |

👉 Mỗi đáp án **chỉ map 1 level duy nhất**

---

## 4. BẮT BUỘC PHẢI CÓ ĐÁNH ĐỔI (TRADE-OFF)

Mỗi câu hỏi phải tạo ra **mâu thuẫn ngầm** giữa ít nhất 2 yếu tố:

- Tự chủ ↔ An toàn
- Nhanh ↔ Chất lượng
- Chủ động ↔ Né trách nhiệm
- Sáng kiến ↔ Tuân thủ

❌ Không có trade-off → CÂU HỎI KHÔNG HỢP LỆ

---

## 5. PHỦ LEVEL (LEVEL COVERAGE)

### 5.1 Số lượng câu tối thiểu

Mỗi level cần:
- Ít nhất **4 câu**
- Ở **các bối cảnh khác nhau**

Ví dụ Level 3:
- Bug nhỏ
- Task mới
- Yêu cầu mơ hồ
- Deadline gấp

---

### 5.2 Tuyệt đối không kết luận từ 1 câu

- Không suy level từ 1 câu
- Không suy level từ 1 tình huống

👉 **Phải nhìn xu hướng hành vi**

---

## 6. CƠ CHẾ CHỐNG “DIỄN”

### 6.1 Câu hỏi đảo chiều

Ít nhất **25% câu hỏi** phải là câu đảo.

Ví dụ:
- Câu 1: Bạn có khả năng làm nhất là gì?
- Câu 6: Khi nào bạn KHÔNG làm điều đó?

---

### 6.2 Thay đổi bối cảnh

Cùng 1 hành vi nhưng đặt trong:
- Rủi ro thấp
- Rủi ro cao
- Phạm vi rõ
- Phạm vi mơ hồ

👉 Không nhất quán = giảm độ tin cậy level

---

## 7. QUY TẮC CHẤM (AI PHẢI TUÂN THEO)

### 7.1 KHÔNG dùng điểm số

❌ Không cộng điểm  
❌ Không lấy trung bình  

---

### 7.2 Quy tắc “Hành vi cao nhất nhưng nhất quán”

- Gom câu trả lời theo level
- Level được coi là đạt khi:
  - ≥ 70% lựa chọn thuộc level đó
  - Trải trên ≥ 3 bối cảnh khác nhau

👉 Level cao nhất thỏa điều kiện = level thực tế

---

## 8. YÊU CẦU REPORT KẾT QUẢ

### 8.1 Cấu trúc report (theo từng skill)

Report bắt buộc có:
1. Tên skill
2. Level suy luận
3. Bằng chứng hành vi (bullet)
4. Giới hạn hiện tại
5. Gợi ý phát triển

---

### 8.2 Ngôn ngữ bị cấm trong report

❌ Lười  
❌ Thiếu động lực  
❌ Thái độ kém  

✅ Chỉ mô tả **hành vi quan sát được**

---

## 9. CÂU HỎI MẪU (THAM KHẢO)

> Có bug nhỏ, tài liệu đầy đủ,  
> deadline trong ngày, leader đang bận.

**Chọn hành vi bạn CÓ KHẢ NĂNG làm nhất trong thực tế:**

- A. Chờ leader xác nhận trước khi sửa  
  *(L2 – phụ thuộc)*

- B. Tự sửa theo guideline và báo kết quả  
  *(L3 – tự chủ trong phạm vi)*

- C. Sửa nhanh phần thấy rõ, test sau  
  *(L2– ưu tiên tốc độ)*

- D. Nhờ đồng nghiệp cùng xử lý cho chắc  
  *(L2.5 – dựa đồng nghiệp)*

---

## 10. RÀNG BUỘC KHI AI SINH CÂU HỎI (BẮT BUỘC)

AI PHẢI:
- Sinh tình huống dựa trên hành vi
- Có trade-off rõ ràng
- Giữ tính thực tế
- Không lộ level SFIA cho người làm

AI KHÔNG ĐƯỢC:
- Hỏi kiến thức
- Nêu “best practice”
- Thưởng đáp án “đẹp”

---

## 11. KHẲNG ĐỊNH CUỐI

Nếu tuân thủ tài liệu này:
- Người làm bài khó “diễn”
- Level suy ra phản ánh đúng thực tế
- Kết quả có giá trị quản trị & phát triển

Nếu vi phạm:
- Bài test biến thành kiểm tra kiến thức
- Kết quả không có giá trị dự đoán

