using SqlSugar;

namespace AqiChartServer.DB
{
    public class SqlSugarHelper //不能是泛型类
    {
        public static string ConnectionString = string.Empty;
        private static SqlSugarScope _db;
        //用单例模式
        public static SqlSugarScope Db {
            get
            {
                if (_db == null)
                {
                    _db = new SqlSugarScope(new ConnectionConfig()
                    {
                        ConnectionString = ConnectionString,//连接符字串
                        DbType = DbType.MySql,
                        IsAutoCloseConnection = true //不设成true要手动close
                    },
                    db => {
                        db.Aop.OnLogExecuting = (sql, pars) =>
                        {
                            //获取原生SQL推荐 5.1.4.63  性能OK
                            Console.WriteLine(UtilMethods.GetNativeSql(sql, pars));

                        };

                    });
                }
                return _db;
            }
        }

    }

    //    //建库
    //    SqlSugarHelper.Db.DbMaintenance.CreateDatabase();//达梦和Oracle不支持建库

    ////建表 （看文档迁移）
    //SqlSugarHelper.Db.CodeFirst.InitTables<Student>(); //所有库都支持

    ////查询表的所有
    //var list = SqlSugarHelper.Db.Queryable<Student>().ToList();

    //    //插入
    //    SqlSugarHelper.Db.Insertable(new Student() { SchoolId = 1, Name = "jack" }).ExecuteCommand();

    //    //更新
    //    SqlSugarHelper.Db.Updateable(new Student() { Id = 1, SchoolId = 2, Name = "jack2" }).ExecuteCommand();

    //    //删除
    //    SqlSugarHelper.Db.Deleteable<Student>().Where(it => it.Id == 1).ExecuteCommand();
}
