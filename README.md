Create the following index first:
```
CREATE NONCLUSTERED INDEX [IX_PersistedGrants_CreationTime] ON [dbo].[PersistedGrants]
(
	[CreationTime] ASC
)WITH (ONLINE = ON) ON [PRIMARY]
GO
```
