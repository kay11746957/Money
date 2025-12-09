const { createApp } = Vue;

createApp({
    data() {
        return {
            form: {
                monthlyExpense: 30000,
                yearsToRetire: 20,
                yearsInRetirement: 30,
                currentAssets: 1000000,
                annualReturn: 5,
                inflationRate: 2,
                includeLaborInsurance: true,
                laborCalcMethod: 'simple',
                avgSalary: 45800,
                insuredYears: 30,
                laborInsuranceAmount: 20000,
                includeLaborPension: true,
                laborPensionAmount: 10000,
            },
            returnRates: [3, 4, 5, 6, 7, 8, 10],
            inflationRates: [1, 2, 3, 4],
            salaryLevels: [26400, 30300, 33300, 36300, 40100, 45800, 48200, 50600, 53000, 55400, 60800, 63800, 66800, 72800, 76500, 80200],
            loading: false,
            result: null,
        };
    },
    computed: {
        // 勞保年金簡易計算：平均薪資 × 年資 × 1.55%
        laborInsuranceMonthly() {
            if (this.form.laborCalcMethod === 'simple') {
                return Math.round(this.form.avgSalary * this.form.insuredYears * 0.0155);
            }
            return this.form.laborInsuranceAmount;
        },
        // 實際使用的勞保金額
        effectiveLaborInsurance() {
            if (!this.form.includeLaborInsurance) return 0;
            return this.laborInsuranceMonthly;
        },
        // 實際使用的勞退金額
        effectiveLaborPension() {
            if (!this.form.includeLaborPension) return 0;
            return this.form.laborPensionAmount;
        },
    },
    methods: {
        calculate() {
            this.loading = true;

            setTimeout(() => {
                // 計算邏輯
                const realReturn = (this.form.annualReturn - this.form.inflationRate) / 100;
                const monthlyReturn = realReturn / 12;

                // 退休所需總額（使用 4% 法則的變形）
                const annualExpense = this.form.monthlyExpense * 12;
                const totalPension = (this.effectiveLaborInsurance + this.effectiveLaborPension) * 12;
                const annualGap = annualExpense - totalPension;

                // 退休後需要的總資產（考慮提領年數）
                const monthsInRetirement = this.form.yearsInRetirement * 12;
                let totalNeeded = 0;
                if (monthlyReturn > 0) {
                    totalNeeded = (annualGap / 12) * ((1 - Math.pow(1 + monthlyReturn, -monthsInRetirement)) / monthlyReturn);
                } else {
                    totalNeeded = (annualGap / 12) * monthsInRetirement;
                }

                // 現有資產在退休時的價值
                const monthsToRetire = this.form.yearsToRetire * 12;
                const futureCurrentAssets = this.form.currentAssets * Math.pow(1 + monthlyReturn, monthsToRetire);

                // 還需要多少
                const gap = Math.max(0, totalNeeded - futureCurrentAssets);

                // 每月需要投資多少
                let monthlyInvestment = 0;
                if (monthlyReturn > 0 && monthsToRetire > 0) {
                    monthlyInvestment = gap * monthlyReturn / (Math.pow(1 + monthlyReturn, monthsToRetire) - 1);
                } else if (monthsToRetire > 0) {
                    monthlyInvestment = gap / monthsToRetire;
                }

                this.result = {
                    monthlyInvestment: Math.round(monthlyInvestment),
                    totalNeeded: Math.round(totalNeeded),
                    monthlyPension: this.effectiveLaborInsurance + this.effectiveLaborPension,
                    gap: Math.round(gap),
                };

                this.loading = false;
            }, 500);
        },
        formatCurrency(value) {
            return new Intl.NumberFormat('zh-TW', {
                style: 'currency',
                currency: 'TWD',
                minimumFractionDigits: 0
            }).format(value);
        },
        formatNumber(value) {
            return new Intl.NumberFormat('zh-TW').format(value);
        },
    },
}).mount('#app');
