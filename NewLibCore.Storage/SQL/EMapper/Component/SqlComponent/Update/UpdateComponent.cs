using Microsoft.Extensions.Options;
using NewLibCore.Storage.SQL.EMapper;
using NewLibCore.Storage.SQL.EMapper.Parser;
using NewLibCore.Storage.SQL.Extension;
using NewLibCore.Storage.SQL.Template;
using NewLibCore.Validate;

namespace NewLibCore.Storage.SQL.Component.Sql
{
    public class UpdateComponent
    {
        internal EntityBase Model { get; set; }

        internal WhereComponent WhereComponent { get; private set; }

        internal FromComponent FromComponent { get; private set; }

        private readonly TemplateBase _templateBase;
        private readonly ConditionProcessor _conditionProcessor;
        private readonly EntityMapperOptions _entityMapperOptions;

        public UpdateComponent(TemplateBase templateBase, MapperDbContextBase mapperDbContextBase, IOptions<EntityMapperOptions> options)
        {
            _templateBase = templateBase;
            _conditionProcessor = new DefaultConditionProcessor(templateBase, new ProcessExecutor(mapperDbContextBase));
            _entityMapperOptions = options.Value;
        }

        internal void AddModel<TModel>(TModel model) where TModel : EntityBase, new()
        {
            Check.IfNullOrZero(model);
            Model = model;
        }

        internal void AddWhereComponent(WhereComponent whereComponent)
        {
            Check.IfNullOrZero(whereComponent);
            WhereComponent = whereComponent;
        }

        internal void AddFromComponent(FromComponent fromComponent)
        {
            Check.IfNullOrZero(fromComponent);
            FromComponent = fromComponent;
        }

        internal SqlExecuteResultConvert Execute()
        {
            return RunDiagnosis.Watch(() =>
            {
                var instance = Model;
                instance.SetUpdateTime();

                if (_entityMapperOptions.EnableModelValidate)
                {
                    instance.CheckPropertyValue();
                }

                var (_, aliasName) = instance.GetEntityBaseAliasName();
                var update = _templateBase.CreateUpdate(instance);
                var result = _conditionProcessor.Process(null, WhereComponent, FromComponent);

                result.Append($@"{update} {PredicateType.AND} {aliasName}.{nameof(instance.IsDeleted)} = 0 {_templateBase.AffectedRows}");
                instance.Reset();

                return result.Execute();
            });
        }
    }
}