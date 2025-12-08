const { createApp } = Vue;

createApp({
    data() {
        return {
            form: {
                monthlyInvestment: 10000,
                annualRate: 6,
                years: 20,
            },
            annualRates: [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 15, 20],
            errors: {
                monthlyInvestment: null,
                years: null,
            },
            loading: false,
            result: null,

        };
    },
    computed: {
        isValid() {
            return !this.errors.monthlyInvestment && !this.errors.years;
        },
    },
    watch: {
        'form.monthlyInvestment'(newValue) {
            this.validateMonthlyInvestment(newValue);
        },
        'form.years'(newValue) {
            this.validateYears(newValue);
        },
    },
    methods: {
        async calculate() {
            if (!this.isValid) return;

            this.loading = true;

            try {
                const response = await fetch('/Home/Calculate', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(this.form),
                });

                if (response.ok) {
                    const data = await response.json();
                    this.result = data;

                } else {
                    console.error('Calculation failed');
                    alert('計算失敗，請稍後再試');
                }
            } catch (error) {
                console.error('Error:', error);
                alert('發生錯誤，請稍後再試');
            } finally {
                this.loading = false;
            }
        },
        formatCurrency(value) {
            return new Intl.NumberFormat('zh-TW', { style: 'currency', currency: 'TWD', minimumFractionDigits: 0 }).format(value);
        },
        validateMonthlyInvestment(value) {
            if (value <= 0) {
                this.errors.monthlyInvestment = '月投入金額必須大於 0 元';
            } else {
                this.errors.monthlyInvestment = null;
            }
        },
        validateYears(value) {
            if (value < 1) {
                this.errors.years = '投資年數至少 1 年';
            } else if (value > 50) {
                this.errors.years = '投資年數最多 50 年';
            } else {
                this.errors.years = null;
            }
        }
    },
    mounted() {
        // Validate initial values
        this.validateMonthlyInvestment(this.form.monthlyInvestment);
        this.validateYears(this.form.years);

        // Add a link to Bootstrap Icons
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css';
        document.head.appendChild(link);

        // Manually bind button click event (workaround for Vue event binding issue)
        this.$nextTick(() => {
            const button = document.querySelector('button[type="submit"]');
            if (button) {
                button.onclick = (e) => {
                    e.preventDefault();
                    this.calculate();
                };
            }
        });
    }
}).mount('#app');