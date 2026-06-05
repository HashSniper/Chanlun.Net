# Agent 工作规范

## 数据安全红线

### EF Core Migration 原则
- **严禁**在做 migration 时删除数据库或旧迁移来重建
- **严禁**使用 `DROP DATABASE` 或 `rm -rf Migrations` 来强制重建
- 正确做法：每次只添加增量迁移，让 EF Core 自动执行 `ALTER TABLE` / `CREATE TABLE`
- 即：
  ```bash
  dotnet ef migrations add <MigrationName> --project Stock.Data
  dotnet ef database update --project Stock.Data
  ```
