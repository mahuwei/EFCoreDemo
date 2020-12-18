# 测试内容

由于一般保存时需要判断进行数据判断，需要重新从数据库读取，所以就涉及到是否跟踪的问题

## 1. 迁移脚本的生产

## 2. 新增数据

- **可以**

### 2.1 主从表一次提交保存

- **可以**

### 2.2 主表已存在，在其子表中添加数据，是否可以保存？

- 不跟踪
- 跟踪：即从数据库去读后，添加从表数据并保存
- **都不可以！！**
  - 错误信息：Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException: Database operation expected to affect 1 row(s) but actually affected 0 row(s).
  - 生成 sql 语句是："update 子表"不是 "insert 子表”
  - **主表已存在，添加子表时只能单独添加**

## 3. 修改数据

### 3.1 总从表一并修改

- 不跟踪：**可以**
- 跟踪：**不行** 除非跟踪后逐个字段进行修改（这样修改不需要调用 DbContext.Entities.Update(...) 方法，直接保存即可）
  - 错误提示：The instance of entity type 'Business' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked.
  - **需要在读取时添加".AsNoTracking()“扩展**

### 3.2 总从表各自修改

- 不跟踪：
- 跟踪：
- **不需要写测试代码了，跟踪不行**

## 4. 删除数据库

- **可以**

  ```sql
  await cd.Database.EnsureDeletedAsync(cts.Token);
  ```
