﻿-- --------------------------------------------------
-- Create users
-- --------------------------------------------------

USE [DictionaryStore];
GO
CREATE USER lucyAppUser FOR LOGIN lucyAppUser;
GO
exec sp_addrolemember 'db_owner', 'lucyAppUser';

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
