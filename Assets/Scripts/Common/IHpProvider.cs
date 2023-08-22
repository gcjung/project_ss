using System;

public interface IHpProvider
{
    public event Action<double, double> OnHealthChanged;
}
