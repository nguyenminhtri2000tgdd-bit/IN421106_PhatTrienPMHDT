# Nhiệm vụ 5 – Nhánh, ≥ 5 commit, rebase, Pull Request (15 điểm)

## 1. Tạo nhánh
```bash
git switch main
git pull
git switch -c feature/solid-refactor
```

## 2. Chia commit theo Conventional Commits (≥ 5; mỗi commit một thay đổi có ý nghĩa)
Sau mỗi bước chạy `dotnet build Ch02.Refactor.sln` rồi mới commit. Gợi ý thứ tự (đều ra từ bài giải):

```bash
# (a) Phân tích vi phạm
git add docs/solid-analysis.md
git commit -m "docs(solid): analyze SOLID violations in ClubManager"

# (b) Domain: chuyển luật nghiệp vụ vào Club/Event
git add Ch02.Refactor/Domain
git commit -m "feat(domain): implement Club.Create, HasRoom, Event.Register and CheckIn"

# (c) Interface nhỏ + hạ tầng (SRP, ISP, DIP)
git add Ch02.Refactor/Abstractions Ch02.Refactor/Infrastructure Ch02.Refactor/GlobalUsings.cs
git commit -m "refactor(infra): extract interfaces, loggers, repositories and notification services"

# (d) Tách service + 4 pattern
git add Ch02.Refactor/Services Ch02.Refactor/Patterns
git commit -m "refactor(services): split ClubManager into ClubService, EventService, ReportService"

# (e) Composition root; xoá God class
git add Ch02.Refactor/Program.cs
git rm Ch02.Refactor/ClubManager.cs
git commit -m "refactor(app): wire dependencies in Program.cs and remove ClubManager"

# (f) Unit test
git add Ch02.Refactor.Tests
git commit -m "test(domain): add xUnit tests following Arrange-Act-Assert"
```
Quy tắc: `loại(phạm vi): mô tả ngắn, thể mệnh lệnh` (`add`, `split`; không dùng `added`). Loại: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`.
Nếu commit lộn xộn: `git rebase -i HEAD~3` để gộp (squash) hoặc đổi tên.

## 3. Rebase lên main mới nhất
```bash
git fetch origin
git switch feature/solid-refactor
git rebase origin/main
```
Nếu có conflict: `git status` → mở file bị conflict → chọn nội dung đúng → xoá dấu `<<<<<<<`, `=======`, `>>>>>>>` → `git add <file>` → `git rebase --continue` (hoặc `git rebase --abort`). Sau đó chạy lại `dotnet build` và `dotnet test`.

Đẩy nhánh (lịch sử đã đổi nên dùng `--force-with-lease`, **không dùng `--force` trần**):
```bash
git push --force-with-lease -u origin feature/solid-refactor
```

## 4. Mở Pull Request – mẫu mô tả (What / Why / How to test)
**Tiêu đề:** `refactor: split ClubManager into services`

```markdown
## What
Tách ClubManager (God class) thành ClubService, EventService, ReportService;
tách logger, thông báo, kho dữ liệu thành các interface nhỏ.
Áp dụng Factory (NotificationServiceFactory), Decorator (LoggingClubRepository),
Strategy (IReportStrategy: Console/CSV), Observer (IEventObserver: Log/Email).
Hoàn thiện Domain (Club.Create, HasRoom, Event.Register, CheckIn) và thêm unit test xUnit.

## Why
ClubManager vi phạm SRP (ôm 5 việc), OCP (if/else theo email), DIP (gọi thẳng File/Console, tự new).
Sau refactor: dễ đọc, dễ mở rộng (thêm kênh SMS/Slack không sửa code cũ), dễ test.
Chi tiết trong docs/solid-analysis.md.

## How to test
dotnet build Ch02.Refactor.sln
dotnet test Ch02.Refactor.sln
dotnet run --project Ch02.Refactor

## Ghi chú
Conflict (nếu có): nêu file và cách xử lý.
```

## 5. Để lại ≥ 2 nhận xét review ngay trên PR của mình
Tab **Files changed** → bấm dấu `+` cạnh dòng code để comment theo dòng. Ví dụ (hãy viết lại bằng lời của bạn, có lý do và gợi ý cách sửa):

1. Trên `Program.cs` (dòng tạo `LoggingClubRepository`): *“Chỉ nơi này dùng `new` cho class cụ thể (composition root). Muốn bỏ log repository chỉ cần bỏ lớp bọc, service không đổi. Gợi ý cải thiện: Chương 3 giao việc ráp này cho DI container.”*
2. Trên `NotificationServiceFactory.Create`: *“Thêm kênh mới vẫn phải thêm một nhánh `switch` ở đây, nhưng thay đổi được cô lập vào một chỗ và nơi gọi không đổi. Đây là gợi ý cải thiện, không phải lỗi phải sửa.”*

Phân biệt **lỗi phải sửa** với **gợi ý cải thiện**; góp ý về code, không về con người. Sau khi sửa, push commit mới và trả lời từng nhận xét.

## 6. Nộp bài
Gửi link Pull Request theo hướng dẫn trên LMS.
