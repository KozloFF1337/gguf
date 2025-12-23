Altair.Services.DBLoadService[0]
      ❌ Ошибка при загрузке данных
      Npgsql.PostgresException (0x80004005): 23502: значение NULL в столбце "KPD" отношения "Boilers" нарушает ограничение NOT NULL

      DETAIL: Detail redacted as it may contain sensitive data. Specify 'Include Error Detail' in the connection string to include this information.
         at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
         at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteNonQuery()
         at Altair.Services.DBLoadService.CalculateYearlyData(NpgsqlConnection pgConn) in C:\Users\KozlovDN\Downloads\С# Projects 3\С# Projects\Altair\Services\DBLoadService.cs:line 522
         at Altair.Services.DBLoadService.<>c__DisplayClass4_0.<ExecuteLoadWithPeriodAsync>b__0() in C:\Users\KozlovDN\Downloads\С# Projects 3\С# Projects\Altair\Services\DBLoadService.cs:line 96      
         at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
      --- End of stack trace from previous location ---
         at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
         at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
      --- End of stack trace from previous location ---
         at Altair.Services.DBLoadService.ExecuteLoadWithPeriodAsync(DateTime startDate, DateTime endDate) in C:\Users\KozlovDN\Downloads\С# Projects 3\С# Projects\Altair\Services\DBLoadService.cs:line 44
        Exception data:
          Severity: ОШИБКА
          SqlState: 23502
          MessageText: значение NULL в столбце "KPD" отношения "Boilers" нарушает ограничение NOT NULL
          Detail: Detail redacted as it may contain sensitive data. Specify 'Include Error Detail' in the connection string to include this information.
          SchemaName: public
          TableName: Boilers
          ColumnName: KPD
          File: execMain.c
          Line: 1988
          Routine: ExecConstraints
info: Altair.Services.AutoLoadDataService[0]
      Автозагрузка данных из АСТЭП завершена успешно
