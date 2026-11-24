let catalog;
const instances = new WeakMap();
const collator = new Intl.Collator('th');

function options(select, entries, value, placeholder) {
    select.replaceChildren(new Option(placeholder, ''));
    for (const entry of entries) select.add(new Option(entry.name, entry.code));
    select.value = entries.some(entry => entry.code === value) ? value : '';
    select.disabled = entries.length === 0;
}

function bind(container) {
    if (instances.has(container)) return instances.get(container);
    const province = container.querySelector('[name="ProvinceCode"]');
    const district = container.querySelector('[name="DistrictCode"]');
    const subdistrict = container.querySelector('[name="SubdistrictCode"]');
    const postal = container.querySelector('[name="PostalCode"]');
    const error = container.querySelector('[data-address-region-error]');
    catalog ??= fetch(container.dataset.source).then(response => {
        if (!response.ok) throw new Error('Address catalog unavailable');
        return response.json();
    }).catch(error => {
        catalog = null;
        throw error;
    });
    const ready = catalog.then(provinces => {
        const alphabetical = entries => [...entries].sort((a, b) => collator.compare(a.name, b.name));
        const set = (value = {}) => {
            options(province, alphabetical(provinces), value.ProvinceCode, 'เลือกจังหวัด');
            const districts = provinces.find(entry => entry.code === province.value)?.districts ?? [];
            options(district, alphabetical(districts), value.DistrictCode, 'เลือกอำเภอ / เขต');
            const subdistricts = districts.find(entry => entry.code === district.value)?.subdistricts ?? [];
            options(subdistrict, alphabetical(subdistricts), value.SubdistrictCode, 'เลือกตำบล / แขวง');
            const postalCodes = subdistricts.find(entry => entry.code === subdistrict.value)?.postalCodes ?? [];
            options(postal, postalCodes.map(code => ({ code, name: code })),
                value.PostalCode || (postalCodes.length === 1 ? postalCodes[0] : ''), 'เลือกรหัสไปรษณีย์');
        };
        province.addEventListener('change', () => set({ ProvinceCode: province.value }));
        district.addEventListener('change', () => set({ ProvinceCode: province.value, DistrictCode: district.value }));
        subdistrict.addEventListener('change', () => set({
            ProvinceCode: province.value, DistrictCode: district.value, SubdistrictCode: subdistrict.value
        }));
        error.hidden = true;
        return set;
    }).catch(() => {
        error.textContent = 'โหลดข้อมูลพื้นที่ไม่สำเร็จ กรุณาเปิดฟอร์มอีกครั้ง';
        error.hidden = false;
        instances.delete(container);
        return () => {};
    });
    instances.set(container, ready);
    return ready;
}

export function applyAddressRegion(form, value) {
    const container = form?.querySelector('[data-address-region]');
    if (container) return bind(container).then(set => set(value));
}

export function readAddressRegion(form) {
    const { ProvinceCode, DistrictCode, SubdistrictCode, PostalCode } = form.elements;
    return { provinceCode: ProvinceCode.value, districtCode: DistrictCode.value, subdistrictCode: SubdistrictCode.value, postalCode: PostalCode.value };
}
