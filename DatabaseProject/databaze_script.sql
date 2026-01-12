
-- admin, který zpracovává požadavek
create table Administrator (
    id int primary key identity(1,1),
    first_name varchar(50) not null,
    last_name varchar(50) not null,
    email varchar(100) not null unique check (email like '%_@_%._%'),
	admin_role varchar(50) not null default 'AIAgent' check(admin_role in ('AIAgent', 'Support', 'SuperAdmin'))
);

-- zakaznik posle udaje
create table Contact (
    id int primary key identity(1,1),
    first_name varchar(50) not null,
    last_name varchar(50) not null,
    email varchar(100) not null unique check (email like '%_@_%._%')
);

-- status pozadavku (ciselnik)
create table RequestStatus (
    id int primary key identity(1,1),
    status_text varchar(20) not null
);

-- pozadavek - zprava, kterou posle zakaznik
create table ServiceRequest (
    id int primary key identity(1,1),
    contact_id int not null foreign key references Contact(id),
    status_id int not null foreign key references RequestStatus(id),
    request_text varchar(max) not null,
    created_date datetime not null default getdate()
);

-- zpracovani adminem 
create table RequestProcessing (
    id int primary key identity(1,1),
    admin_id int foreign key references Administrator(id),
    request_id int not null foreign key references ServiceRequest(id),
    started_date datetime not null default getdate(),
	ended_date datetime default null,
	check (ended_date is null or ended_date >= started_date),
	response_text varchar(max)

);
go
create or alter view view_request_overview as
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
go
go
create or alter view view_admin_statistics as
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
go

use tesar2;
select * from Administrator;
select * from Contact;
select * from RequestProcessing;
select * from RequestStatus;
select * from ServiceRequest;

delete from RequestProcessing;
delete from ServiceRequest;
delete from Contact;
delete from Administrator;

select * from view_admin_statistics
select * from view_request_overview
