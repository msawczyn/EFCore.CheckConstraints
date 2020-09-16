using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using JetBrains.Annotations;

namespace EFCore.CheckConstraints.Internal
{
    public class CheckConstraintsOptionsExtension : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;
        private bool _enumCheckConstraintsEnabled;
        private bool _discriminatorCheckConstraintsEnabled;

        public CheckConstraintsOptionsExtension() {}

        protected CheckConstraintsOptionsExtension([NotNull] CheckConstraintsOptionsExtension copyFrom)
        {
            _enumCheckConstraintsEnabled = copyFrom._enumCheckConstraintsEnabled;
            _discriminatorCheckConstraintsEnabled = copyFrom._discriminatorCheckConstraintsEnabled;
        }

        public virtual DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

        protected virtual CheckConstraintsOptionsExtension Clone() => new CheckConstraintsOptionsExtension(this);

        public virtual bool AreEnumCheckConstraintsEnabled => _enumCheckConstraintsEnabled;
        public virtual bool AreDiscriminatorCheckConstraintsEnabled => _discriminatorCheckConstraintsEnabled;

        public virtual CheckConstraintsOptionsExtension WithEnumCheckConstraintsEnabled(bool enumCheckConstraintsEnabled)
        {
            var clone = Clone();
            clone._enumCheckConstraintsEnabled = enumCheckConstraintsEnabled;
            return clone;
        }

        public virtual CheckConstraintsOptionsExtension WithDiscriminatorCheckConstraintsEnabled(
            bool discriminatorCheckConstraintsEnabled)
        {
            var clone = Clone();
            clone._discriminatorCheckConstraintsEnabled = discriminatorCheckConstraintsEnabled;
            return clone;
        }

        public void Validate(IDbContextOptions options) {}

        public void ApplyServices(IServiceCollection services)
            => services.AddEntityFrameworkCheckConstraints();

        internal sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            private string _logFragment;

            public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) {}

            private new CheckConstraintsOptionsExtension Extension
                => (CheckConstraintsOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider => false;

            public override string LogFragment
            {
                get
                {
                    if (_logFragment == null)
                    {
                        var builder = new StringBuilder("using check constraints (");
                        var isFirst = true;

                        if (Extension.AreEnumCheckConstraintsEnabled)
                        {
                            builder.Append("enums");
                            isFirst = false;
                        }

                        if (Extension.AreDiscriminatorCheckConstraintsEnabled)
                        {
                            if (!isFirst)
                                builder.Append(", ");
                            builder.Append("discriminators");
                        }

                        builder.Append(')');

                        _logFragment = builder.ToString();
                    }

                    return _logFragment;
                }
            }

            public override long GetServiceProviderHashCode()
                => HashCode.Combine(
                    Extension._enumCheckConstraintsEnabled.GetHashCode(),
                    Extension._discriminatorCheckConstraintsEnabled.GetHashCode());

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                debugInfo["CheckConstraints:Enums"]
                    = Extension._enumCheckConstraintsEnabled.GetHashCode().ToString(CultureInfo.InvariantCulture);
                debugInfo["CheckConstraints:Discriminators"]
                    = Extension._discriminatorCheckConstraintsEnabled.GetHashCode().ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}