﻿using CsvHelper;
using SpendingInfo.Transactions.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SpendingInfo.Transactions.Tables
{
    public class TransactionTable<T> : ObservableCollection<T>, ICollection<T>, IEnumerable<T> where T : ITransaction
    {
        IList<T> allTransactions = new List<T>();
        HashSet<string> transactionIDs = new HashSet<string>();

        public void AddTransactions(IEnumerable<T> transactions)
        {
            Func<T, bool> idNotExists = t => !transactionIDs.Contains(t.GetID());
            foreach (T t in transactions.Where(idNotExists))
            {
                Add(t);
                allTransactions.Add(t);
            }
        }

        public IEnumerable<T> EnumerateTransactions()
        {
            foreach (T transaction in allTransactions)
            {
                yield return transaction;
            }
        }

        public void RemoveTransaction(T t)
        {
            Remove(t);
            transactionIDs.Remove(t.GetID());
            allTransactions.Remove(t);
        }

        public ICollection<T> GetAllTransactions()
        {
            return allTransactions;
        }

        public virtual void SelectWithDates(DateTime start, DateTime end)
        {
            Clear();
            Func<T, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(InRange);
            foreach (T t in sourceEnumerable) Add(t);
        }

        public virtual void SearchByDescription(string query)
        {
            Clear();
            Func<T, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(searchFunc);
            foreach (T t in sourceEnumerable) Add(t);
        }

        public virtual void SelectWithinDatesAndSearch(DateTime start, DateTime end, string query)
        {
            Clear();
            Func<T, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            Func<T, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            IEnumerable<T> sourceEnumerable = from t in GetAllTransactions() where searchFunc(t) && InRange(t) select t;
            foreach (T t in sourceEnumerable) Add(t);
        }

        public ICollection<T> GetSelectedTransactions() => this;

        public string SerializeJson()
        {
            var options = new JsonSerializerOptions { };
            return SerializeJson(options);
        }

        public string SerializeJson(JsonSerializerOptions options)
        {
            var selectedTransactions = GetSelectedTransactions();
            return JsonSerializer.Serialize(selectedTransactions, options);
        }

        public void SaveToJson(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            SaveToJson(path, options);
        }

        public async void SaveToJson(string path, JsonSerializerOptions options)
        {
            using FileStream fileStream = File.Create(path);
            var transactions = GetSelectedTransactions();
            await JsonSerializer.SerializeAsync(fileStream, transactions, options);
            await fileStream.DisposeAsync();
        }

        public static TransactionTable<T> LoadFromJson(string path)
        {
            var options = new JsonSerializerOptions { };
            return LoadFromJson(path, options);
        }

        public static TransactionTable<T> LoadFromJson(string path, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public void ClearAll()
        {
            Clear();
            transactionIDs.Clear();
            allTransactions.Clear();
        }

        public virtual void ExportSelectedAsCSV(string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                ExportSelectedAsCSV(fileStream);
            }
        }

        public virtual void ExportSelectedAsCSV(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                ExportSelectedAsCSV(csv);
            }
        }

        public virtual void ExportSelectedAsCSV(CsvWriter csv)
        {
            throw new NotImplementedException("Each implementing class of TransactionTable must implement this function");
        }

    }
}