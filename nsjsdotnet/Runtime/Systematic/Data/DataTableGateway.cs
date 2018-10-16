namespace nsjsdotnet.Runtime.Systematic.Data
{
    using nsjsdotnet.Core;
    using System;
    using System.Data;
    using System.Windows.Forms;
    using DatabaseAccessAdapter = nsjsdotnet.Core.Data.Database.DatabaseAccessAdapter;
    using DATATableGateway = nsjsdotnet.Core.Data.Database.DataTableGateway;

    sealed class DataTableGateway // Human Legacy(人类遗产)
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private static readonly NSJSFunctionCallback2 g_SelectProc;
        private static readonly NSJSFunctionCallback2 g_ExecuteNonQueryProc;
        private static readonly NSJSFunctionCallback2 g_DeriveParametersProc;
        private static readonly NSJSFunctionCallback2 g_CloseProc;
        private static readonly NSJSFunctionCallback g_BeginTransactionProc;

        static DataTableGateway()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.
                ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(New));
            owner.Set("Invalid", NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(Invalid));
            g_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(Close);
            g_SelectProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(Select);
            g_ExecuteNonQueryProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(ExecuteNonQuery);
            g_DeriveParametersProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(DeriveParameters);
            g_BeginTransactionProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(BeginTransaction);
        }

        private static void Invalid(NSJSFunctionCallbackInfo arguments)
        {
            arguments.SetReturnValue(GetGateway(arguments.This) == null);
        }

        public static void Select(NSJSFunctionCallbackInfo arguments)
        {
            InternalExecute(arguments, false);
        }

        public static void ExecuteNonQuery(NSJSFunctionCallbackInfo arguments)
        {
            InternalExecute(arguments, true);
        }

        public static void DeriveParameters(NSJSFunctionCallbackInfo arguments)
        {
            InternalExecute(arguments, (gateway, adapter, procedure) =>
            {
                try
                {
                    IDbDataParameter[] parameters = adapter.GetParameters(procedure);
                    arguments.SetReturnValue(ArrayAuxiliary.ToArray(arguments.VirtualMachine, parameters));
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
            });
        }

        private static void InternalExecute(NSJSFunctionCallbackInfo arguments, bool nonquery)
        {
            InternalExecute(arguments, (gateway, adapter, text) =>
            {
                DataTable dataTable = null;
                IDbCommand command = null;
                try
                {
                    IDbTransaction transaction = null;
                    NSJSArray parameters = null;
                    NSJSInt32 cmdtype = null;
                    for (int solt = 1, count = arguments.Length; solt < count && (transaction == null ||
                        cmdtype == null || parameters == null); solt++)
                    {
                        NSJSValue current = arguments[solt];
                        if (transaction == null)
                        {
                            transaction = DatabaseTransaction.GetTransaction(current as NSJSObject);
                        }
                        if (cmdtype == null)
                        {
                            cmdtype = current as NSJSInt32;
                        }
                        if (parameters == null)
                        {
                            parameters = current as NSJSArray;
                        }
                    }
                    command = ObjectAuxiliary.ToDbCommand(adapter, text, parameters);
                    int commandType = ValueAuxiliary.ToInt32(cmdtype);
                    switch (commandType)
                    {
                        case 1:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                        case 2:
                            command.CommandType = CommandType.TableDirect;
                            break;
                        default:
                            command.CommandType = CommandType.Text;
                            break;
                    }
                    command.Transaction = transaction;
                    if (nonquery)
                    {
                        arguments.SetReturnValue(gateway.ExecuteNonQuery(command));
                    }
                    else
                    {
                        dataTable = gateway.Select(command);
                        arguments.SetReturnValue(ArrayAuxiliary.ToArray(arguments.VirtualMachine, dataTable));
                    }
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
                if (dataTable != null)
                {
                    dataTable.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
            });
        }

        private class DatabaseTransaction
        {
            private static readonly NSJSFunctionCallback g_RollbackProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Rollback);
            private static readonly NSJSFunctionCallback g_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
            private static readonly NSJSFunctionCallback g_CommitProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Commit);

            public static NSJSObject New(NSJSVirtualMachine machine, IDbTransaction transaction)
            {
                if (machine == null || transaction == null)
                {
                    return null;
                }
                NSJSObject o = NSJSObject.New(machine);
                o.Set("Commit", g_CommitProc);
                o.Set("Rollback", g_RollbackProc);
                o.Set("Close", g_CloseProc);
                o.Set("Dispose", g_CloseProc);
                NSJSKeyValueCollection.Set(o, transaction);
                return o;
            }

            public static void Close(IntPtr info)
            {
                NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
                IDbTransaction transaction;
                NSJSKeyValueCollection.Release(arguments.This, out transaction);
                if (transaction != null)
                {
                    try
                    {
                        transaction.Dispose();
                    }
                    catch (Exception exception)
                    {
                        Throwable.Exception(arguments.VirtualMachine, exception);
                    }
                }
            }

            private static void RollbackOrCommit(IntPtr info, bool commiting)
            {
                ObjectAuxiliary.Call<IDbTransaction>(info, (transaction, arguments) =>
                {
                    try
                    {
                        if (commiting)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception exception)
                    {
                        Throwable.Exception(arguments.VirtualMachine, exception);
                    }
                });
            }

            public static void Rollback(IntPtr info)
            {
                RollbackOrCommit(info, false);
            }

            public static void Commit(IntPtr info)
            {
                RollbackOrCommit(info, true);
            }

            public static IDbTransaction GetTransaction(NSJSObject value)
            {
                return NSJSKeyValueCollection.Get<IDbTransaction>(value);
            }
        }

        private static void InternalExecute(NSJSFunctionCallbackInfo arguments,
            Action<DATATableGateway, DatabaseAccessAdapter, string> executing)
        {
            if (executing == null)
            {
                throw new ArgumentNullException("executing");
            }
            DATATableGateway gateway = GetGateway(arguments.This);
            if (gateway == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else if (arguments.Length <= 0)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                string text = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
                if (text == null)
                {
                    Throwable.ArgumentNullException(arguments.VirtualMachine);
                }
                else
                {
                    executing(gateway, gateway.DatabaseAccessAdapter, text);
                }
            }
        }

        public static DATATableGateway GetGateway(NSJSObject value)
        {
            return NSJSKeyValueCollection.Get<DATATableGateway>(value);
        }

        public static void Close(NSJSFunctionCallbackInfo arguments)
        {
            arguments.SetReturnValue(ObjectAuxiliary.RemoveInKeyValueCollection(arguments.This));
        }

        public static NSJSObject New(NSJSVirtualMachine machine, DATATableGateway gateway)
        {
            if (machine == null || gateway == null)
            {
                return null;
            }
            NSJSObject o = NSJSObject.New(machine);
            o.Set("Select", g_SelectProc);
            o.Set("Close", g_CloseProc);
            o.Set("Dispose", g_CloseProc);
            o.Set("ExecuteNonQuery", g_ExecuteNonQueryProc);
            o.Set("DeriveParameters", g_DeriveParametersProc);
            o.Set("BeginTransaction", g_BeginTransactionProc);
            NSJSKeyValueCollection.Set(o, gateway);
            return o;
        }

        public static void BeginTransaction(IntPtr info)
        {
            ObjectAuxiliary.Call<DATATableGateway>(info, (gateway, arguments) =>
            {
                IDbTransaction transaction = gateway.CreateTransaction();
                NSJSObject objective = DatabaseTransaction.New(arguments.VirtualMachine, transaction);
                arguments.SetReturnValue(objective);
            });
        }

        public static DatabaseAccessAdapter GetAdapter(NSJSObject value)
        {
            return NSJSKeyValueCollection.Get<DatabaseAccessAdapter>(value);
        }

        public static void New(NSJSFunctionCallbackInfo arguments)
        {
            DatabaseAccessAdapter adapter = GetAdapter(arguments.Length > 0 ? arguments[0] as NSJSObject : null);
            if (adapter == null)
            {
                Throwable.ArgumentNullException(arguments.VirtualMachine);
            }
            else
            {
                arguments.SetReturnValue(New(arguments.VirtualMachine, new DATATableGateway(adapter)));
            }
        }
    }
}
