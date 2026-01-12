USE [tesar2]
GO
/****** Object:  Table [dbo].[Administrator]    Script Date: 12.01.2026 1:02:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Administrator](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[first_name] [varchar](50) NOT NULL,
	[last_name] [varchar](50) NOT NULL,
	[email] [varchar](100) NOT NULL,
	[admin_role] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Contact]    Script Date: 12.01.2026 1:02:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Contact](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[first_name] [varchar](50) NOT NULL,
	[last_name] [varchar](50) NOT NULL,
	[email] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequestStatus]    Script Date: 12.01.2026 1:02:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequestStatus](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[status_text] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ServiceRequest]    Script Date: 12.01.2026 1:02:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceRequest](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[contact_id] [int] NOT NULL,
	[status_id] [int] NOT NULL,
	[request_text] [varchar](max) NOT NULL,
	[created_date] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequestProcessing]    Script Date: 12.01.2026 1:02:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequestProcessing](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[admin_id] [int] NULL,
	[request_id] [int] NOT NULL,
	[started_date] [datetime] NOT NULL,
	[ended_date] [datetime] NULL,
	[response_text] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[view_request_overview]    Script Date: 12.01.2026 1:02:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create   view [dbo].[view_request_overview] as
select 
    sr.id as request_id,
    c.first_name as contact_first_name,
    c.last_name as contact_last_name,
    c.email as contact_email,
    sr.request_text,
    rs.status_text as status,
    sr.created_date,

    a.first_name as assigned_admin_first_name,
    a.last_name as assigned_admin_last_name,
    a.email as assigned_admin_email,

    rp.started_date,
    rp.ended_date,
    rp.response_text

from servicerequest sr
join contact c on sr.contact_id = c.id
join requeststatus rs on sr.status_id = rs.id
left join requestprocessing rp on rp.request_id = sr.id
left join administrator a on rp.admin_id = a.id;

GO
/****** Object:  View [dbo].[view_admin_statistics]    Script Date: 12.01.2026 1:02:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create   view [dbo].[view_admin_statistics] as
select 
    a.id as admin_id,
    a.first_name,
    a.last_name,
    a.email,
    a.admin_role,

    count(case when sr.status_id = 1 then 1 end) as new_requests,
    count(case when sr.status_id = 2 then 1 end) as assigned_requests,
    count(case when sr.status_id = 3 then 1 end) as completed_requests,
    count(case when sr.status_id = 4 then 1 end) as cancelled_requests,

    count(distinct rp.request_id) as total_processed,

    avg(
        case 
            when rp.ended_date is not null 
            then datediff(minute, rp.started_date, rp.ended_date)
        end
    ) as avg_processing_time_minutes

from administrator a
left join requestprocessing rp on a.id = rp.admin_id
left join servicerequest sr on rp.request_id = sr.id
group by a.id, a.first_name, a.last_name, a.email, a.admin_role

GO
SET IDENTITY_INSERT [dbo].[RequestStatus] ON 
GO
INSERT [dbo].[RequestStatus] ([id], [status_text]) VALUES (1, N'Novy')
GO
INSERT [dbo].[RequestStatus] ([id], [status_text]) VALUES (2, N'ResiSe')
GO
INSERT [dbo].[RequestStatus] ([id], [status_text]) VALUES (3, N'Uzavreny')
GO
INSERT [dbo].[RequestStatus] ([id], [status_text]) VALUES (4, N'Storno')
GO
SET IDENTITY_INSERT [dbo].[RequestStatus] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Administ__AB6E6164C50F25F1]    Script Date: 12.01.2026 1:02:45 ******/
ALTER TABLE [dbo].[Administrator] ADD UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Contact__AB6E61645019A7A9]    Script Date: 12.01.2026 1:02:45 ******/
ALTER TABLE [dbo].[Contact] ADD UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Administrator] ADD  DEFAULT ('AIAgent') FOR [admin_role]
GO
ALTER TABLE [dbo].[RequestProcessing] ADD  DEFAULT (getdate()) FOR [started_date]
GO
ALTER TABLE [dbo].[RequestProcessing] ADD  DEFAULT (NULL) FOR [ended_date]
GO
ALTER TABLE [dbo].[ServiceRequest] ADD  DEFAULT (getdate()) FOR [created_date]
GO
ALTER TABLE [dbo].[RequestProcessing]  WITH CHECK ADD FOREIGN KEY([admin_id])
REFERENCES [dbo].[Administrator] ([id])
GO
ALTER TABLE [dbo].[RequestProcessing]  WITH CHECK ADD FOREIGN KEY([request_id])
REFERENCES [dbo].[ServiceRequest] ([id])
GO
ALTER TABLE [dbo].[ServiceRequest]  WITH CHECK ADD FOREIGN KEY([contact_id])
REFERENCES [dbo].[Contact] ([id])
GO
ALTER TABLE [dbo].[ServiceRequest]  WITH CHECK ADD FOREIGN KEY([status_id])
REFERENCES [dbo].[RequestStatus] ([id])
GO
ALTER TABLE [dbo].[Administrator]  WITH CHECK ADD CHECK  (([admin_role]='SuperAdmin' OR [admin_role]='Support' OR [admin_role]='AIAgent'))
GO
ALTER TABLE [dbo].[Administrator]  WITH CHECK ADD CHECK  (([email] like '%_@_%._%'))
GO
ALTER TABLE [dbo].[Contact]  WITH CHECK ADD CHECK  (([email] like '%_@_%._%'))
GO
ALTER TABLE [dbo].[RequestProcessing]  WITH CHECK ADD CHECK  (([ended_date] IS NULL OR [ended_date]>=[started_date]))
GO
