﻿using System;
using System.Linq;
using System.Text;
using LightningDB;

namespace SecondProcess {
    class Program {
        static void Main(string[] args) {
            var name = args.First();
            using var env = new LightningEnvironment(name);
            env.Open(EnvironmentOpenFlags.ReadOnly);
            byte[] results;
            using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly)) {
                using var db = tx.OpenDatabase();
                var result = tx.Get(db, Encoding.UTF8.GetBytes("hello"));
                results = result.value.AsSpan().ToArray();
                tx.Commit();
            }

            Console.WriteLine(Encoding.UTF8.GetString(results));
        }
    }
}
