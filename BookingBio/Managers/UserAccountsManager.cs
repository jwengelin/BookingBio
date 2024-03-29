﻿using BookingBio.Models;
using BookingBio.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BookingBio.Managers
{
    public class UserAccountsManager
    {        

        public UserAccounts CreateUserAccount(string accountName, string accountPassword, string phoneNumber, string customerName, Customers cust) // skapa user account
        {
            string newSalt = CreateSalt(10); // Calls Create salt function
            string hashedPass = HashPassword(newSalt, accountPassword); // Calls hashpassword function    
            UserAccounts user = new UserAccounts();
            user.accountName = accountName;
            user.accountPassword = hashedPass;
            user.salt = newSalt;
            user.phoneNumber = phoneNumber;
            user.customerName = customerName;
            user.Customers = cust;


            return user;
        }
        public UserAccounts AddCustomerToUserAccount(string accountNameInput, Customers cust) // lägga till customer till useracount
        {
            var userAcc = new UserAccounts();
            try
            {
                using (var db = new BookingDBEntities())
                {

                    userAcc = (from s in db.UserAccounts
                               where s.accountName == accountNameInput
                               select s).FirstOrDefault<UserAccounts>();

                }
                userAcc.Customers = cust;

                return userAcc;

            } catch
            {
                userAcc = null;
                return userAcc;
            }            
        }

        public UserAccounts UpdateAccountDetails(UpdateAccountDTO userAccInput) // update account details
        {
            UserAccounts dbUpdatedAcc = GetUserAccountByName(userAccInput.AccountName); // gets user entity with account name
            if (dbUpdatedAcc is null)
            {
                return dbUpdatedAcc;
            }
            dbUpdatedAcc.customerName = userAccInput.CustomerName; // Uppdates customer name
            dbUpdatedAcc.phoneNumber = userAccInput.PhoneNumber; // Updates phonenumber
            
            return dbUpdatedAcc;
        }

        public string ChangePassword (string passwordInput, string userAccName)
        {
            string salt = GetUserSalt(userAccName); // Gets userAcc salt
            string hashedPassword = HashPassword(salt, passwordInput); // Hashes and salts new password

            return hashedPassword;
        }

        public bool Login(string accountNameInput, string accountPasswordInput) // Login
        {
            string salt;
            string passwordInDb;
            string hashedPassword;
            bool loginOk = false;
            try
            {
                salt = GetUserSalt(accountNameInput); // Get salt from DB with UserAccountName
                if (salt is null)
                {
                    return loginOk = false;
                }
                passwordInDb = GetPassword(accountNameInput); // Gets hashed password from DB
                hashedPassword = HashPassword(salt, accountPasswordInput); // Concatenates password input and salt and runs algorithm
            }
            catch
            {
                return loginOk; // returns false bool if operations above fails
            }

            if (passwordInDb.Equals(hashedPassword)) // if hashed input password equals hashed password from database, LoginOk becomes true
            {
                loginOk = true;
            }

            return loginOk; 
        }
        public UserAccounts GetUserAccountByName(string accountNameInput) // Gets user entity from db
        {
            try
            {
                using (var db = new BookingDBEntities()) 
                {
                    var userAcc = (from s in db.UserAccounts
                                   where s.accountName == accountNameInput
                                   select s).FirstOrDefault<UserAccounts>();

                    return userAcc;
                }
            } catch
            {
                return null; // returns null if entity is not found
            }
            

        }

        public string GetPassword(string accountName) // Gets password from DB
        {
            using (var db = new BookingDBEntities())
            {
                var password = db.UserAccounts
                              .Where(s => s.accountName == accountName)
                              .Select(s => s.accountPassword)
                              .FirstOrDefault()
                              .ToString();
                return password;
            }
        }

        public string GetUserSalt(string accountNameInput) // Gets salt from DB
        {
            string salt = null;
            using (var db = new BookingDBEntities())
            {
                try
                {
                    salt = db.UserAccounts
                              .Where(s => s.accountName == accountNameInput)
                              .Select(s => s.salt)
                              .FirstOrDefault()
                              .ToString();
                }
                catch
                {
                    return salt;
                }
                
                return salt;
            }
        }

        public bool CheckIfAccountNameExists (string accountNameInput) // Check if account name exists in DB
        {
            bool accountNameIsNotOk = true;
            using (var db = new BookingDBEntities())
            {
                var accName = (from s in db.UserAccounts
                               where s.accountName == accountNameInput 
                               select s.accountName).DefaultIfEmpty(String.Empty).First();

                if (accName.Equals(String.Empty))
                {
                    accountNameIsNotOk = false;
                };

                return accountNameIsNotOk;
            }
        }

        public bool UpdateEntityInDb (object updatedUser)
        {
            bool entityUpdated = false;
            using (var db = new BookingDBEntities())
            {
                db.Entry(updatedUser).State = EntityState.Modified;
                bool saveFailed;
                do
                {
                    saveFailed = false;
                    try
                    {
                        db.SaveChanges(); // saves changes in DB
                        entityUpdated = true;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        saveFailed = true;
                        ex.Entries.Single().Reload(); // reloads entity from DB
                        entityUpdated = false;
                    }

                } while (saveFailed);
            }            
            return entityUpdated;
        }

        public bool CheckIfPasswordIsOk (string password) // Check if password is ok
        {
            bool passwordIsNotOk = true;
            if (password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit))
            {
                passwordIsNotOk = false;
            }
            return passwordIsNotOk;
        }

        public bool CheckIfUserIsLoggedIn(string userInputToken, string loginToken )
        {
            bool loginOk = false;
            

            if (userInputToken is null || userInputToken.Equals("")) // Token name and content?
                {
                    loginOk = false;
                }
            if (userInputToken.Equals(loginToken)) // check if token exists
            {
                loginOk = true;
            }
            return loginOk;
        }

        public int GetAccountIdFromName (string accountName)
        {
            using (var db = new BookingDBEntities())
            {

                var userAccId = (from s in db.UserAccounts
                                 where s.accountName == accountName
                                 select s.userAccountId).FirstOrDefault();

                return userAccId;
            }
        }

        private static string CreateSalt(int size) // Creates salt
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        public string HashPassword(string salt, string password) // Concatenates salt and password
        {
            string mergedPass = string.Concat(salt, password);
            return Sha256(mergedPass);
        }

        private string Sha256(string randomString) // Hashes salted password
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
        public string CreateToken (string accountName)
        {
            UserAccountsManager umgr = new UserAccountsManager();

            string salt = umgr.GetUserSalt(accountName);
            if (salt is null)
            {
                return salt;
            }
            string hashedToken = HashPassword(salt, accountName);
            return hashedToken;
        }
        public int GetCustomerIdFromUserAccountName (string accountName)
        {
            using (var db = new BookingDBEntities())
            {

                var userAccId = (from s in db.UserAccounts
                                 where s.accountName == accountName
                                 select s.customerId).FirstOrDefault();

                return userAccId;
            }
        }

        

        
    }
}