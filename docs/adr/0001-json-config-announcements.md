# JSON Configuration File for Admin Announcements

We decided to persist global admin announcements in a lightweight JSON configuration file (`wwwroot/data/announcement.json`) instead of creating a SQL Server database table. This avoids database schema and migration overhead for a simple key-value configuration that only holds one record at a time. The trade-off is that it cannot scale to concurrent multi-admin concurrent writes, which is acceptable since the gym dashboard has low write frequency.
